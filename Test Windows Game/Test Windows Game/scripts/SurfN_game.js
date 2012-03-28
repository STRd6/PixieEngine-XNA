
var Base;
Base = function(I) {
  var self;
  self = GameObject(I).extend({
    center: function() {
      return Point(I.x, I.y);
    }
  });
  self.unbind("draw");
  self.bind("draw", function(canvas) {
    if (I.sprite) {
      if (I.sprite.draw != null) {
        return I.sprite.draw(canvas, -I.width / 2, -I.height / 2);
      }
    }
  });
  self.bind("drawDebug", function(canvas) {
    var center, x, y;
    if (I.radius) {
      center = self.center();
      x = center.x;
      y = center.y;
      return canvas.fillCircle(x, y, I.radius, "rgba(255, 0, 255, 0.5)");
    }
  });
  return self;
};;
var Cloud;
Cloud = function(I) {
  var self;
  Object.reverseMerge(I, {
    sprite: "cloud",
    height: 32,
    width: 128,
    y: -120 + rand(240),
    zIndex: 1
  });
  self = Base(I);
  self.bind("update", function() {
    var destruction;
    destruction = engine.find(".destruction").first();
    if (destruction) {
      if (I.x < destruction.I.x - I.width) {
        return I.active = false;
      }
    }
  });
  return self;
};;
var GameOver;
GameOver = function(I) {
  var lineHeight, self;
  Object.reverseMerge(I, {
    zIndex: 10
  });
  lineHeight = 24;
  self = GameObject(I).extend({
    draw: function(canvas) {
      canvas.font("bold 24px consolas, 'Courier New', 'andale mono', 'lucida console', monospace");
      canvas.fillColor("#FFF");
      return canvas.withTransform(Matrix.translation(I.x - App.width / 2, 0), function() {
        canvas.centerText("surf'd for " + (I.distance.toFixed(2)) + " meters", I.y - lineHeight);
        canvas.centerText("sur5'd for " + ((I.time / 30).toFixed(2)) + " seconds", I.y);
        return canvas.centerText("succumb'd to " + I.causeOfDeath, I.y + lineHeight);
      });
    }
  });
  self.bind("update", function() {
    if (keydown.space || keydown["return"] || keydown.escape) {
      return engine.trigger("restart");
    }
  });
  return self;
};;
var Player;
Player = function(I) {
  var GRAVITY, angleSprites, land, launch, self, setSprite, sprites, wipeout;
  Object.reverseMerge(I, {
    airborne: true,
    heading: Math.TAU / 4,
    sprite: "player",
    launchBoost: 1.5,
    radius: 8,
    rotationVelocity: Math.TAU / 64,
    waterSpeed: 5,
    velocity: Point(0, 0),
    zIndex: 5
  });
  self = Base(I);
  GRAVITY = Point(0, 0.25);
  sprites = [];
  angleSprites = 8;
  angleSprites.times(function(n) {
    var t;
    t = n * 2;
    return sprites.push(Sprite.loadByName("player_" + t));
  });
  setSprite = function() {
    angleSprites;    var n;
    n = (angleSprites * I.heading / Math.TAU).round().mod(angleSprites);
    return I.sprite = sprites[n];
  };
  wipeout = function(causeOfDeath) {
    I.active = false;
    Sound.play("crash");
    return engine.add({
      "class": "GameOver",
      causeOfDeath: causeOfDeath,
      distance: I.x,
      time: I.age,
      x: I.x,
      y: I.y
    });
  };
  land = function() {
    var _ref, _ref2, _ref3;
    if (I.velocity.x > 1.5) {
      if (!((0 <= (_ref = I.heading) && _ref <= Math.PI / 2))) {
        wipeout("bad landing");
      }
    } else if (I.velocity.x < -1.5) {
      if (!((Math.PI / 2 <= (_ref2 = I.heading) && _ref2 <= Math.PI))) {
        wipeout("bad landing");
      }
    } else {
      if (!((Math.PI / 5 <= (_ref3 = I.heading) && _ref3 <= 4 * Math.PI / 5))) {
        wipeout("bad landing");
      }
    }
    I.airborne = false;
    return Sound.play("land");
  };
  launch = function() {
    I.airborne = true;
    I.velocity.scale$(I.launchBoost);
    return Sound.play("splash");
  };
  self.bind("drawDebug", function(canvas) {
    var p;
    canvas.strokeColor("rgba(0, 255, 0, 0.75)");
    p = Point.fromAngle(I.heading).scale(10);
    return canvas.drawLine(I.x - p.x, I.y - p.y, I.x + p.x, I.y + p.y, 1);
  });
  self.bind("update", function() {
    var circle, depthsLevel, headingChange, hitDestruction, hitRock, speed, waterLevel;
    I.x += I.velocity.x;
    I.y += I.velocity.y;
    I.waterSpeed = 5 + I.age / 200;
    circle = self.circle();
    hitRock = false;
    engine.find("Rock").each(function(rock) {
      if (Collision.circular(circle, rock.circle())) {
        return hitRock = true;
      }
    });
    if (hitRock) {
      wipeout("a rock");
      return;
    }
    hitDestruction = false;
    engine.find(".destruction").each(function(destruction) {
      if (I.x < destruction.I.x) {
        return hitDestruction = true;
      }
    });
    if (hitDestruction) {
      wipeout("a rogue wave");
      return;
    }
    waterLevel = engine.find(".water").first().I.y;
    depthsLevel = waterLevel + 160;
    headingChange = I.rotationVelocity;
    if (I.airborne) {
      headingChange *= 2;
    }
    if (keydown.left) {
      I.heading -= headingChange;
    }
    if (keydown.right) {
      I.heading += headingChange;
    }
    I.heading = I.heading.constrainRotation();
    setSprite();
    if (I.y > depthsLevel) {
      return wipeout("the depths");
    } else if (I.y >= waterLevel) {
      if (I.airborne) {
        land();
      }
      speed = I.velocity.magnitude();
      speed = speed.approachByRatio(I.waterSpeed, 0.1);
      return I.velocity = Point.fromAngle(I.heading).scale(speed);
    } else {
      if (!I.airborne) {
        launch();
      }
      return I.velocity.add$(GRAVITY);
    }
  });
  return self;
};;
var Rock;
Rock = function(I) {
  var self;
  Object.reverseMerge(I, {
    sprite: "rocks",
    height: 32,
    radius: 16,
    width: 32,
    y: 160 + rand(160),
    zIndex: 6
  });
  self = Base(I);
  self.bind("update", function() {
    var destruction;
    destruction = engine.find(".destruction").first();
    if (destruction) {
      if (I.x < destruction.I.x - I.width) {
        return I.active = false;
      }
    }
  });
  return self;
};;

App.entities = {};;
;
var DEBUG_DRAW, churnSprites, clock, depthsSprites, restartGame, setUpGame, waveSprites;
DEBUG_DRAW = false;

engine = Engine({
  backgroundColor: "burntorange",
  canvas: Canvas(),
  zSort: true
});

depthsSprites = [Sprite.loadByName("depths0"), Sprite.loadByName("depths1")];
churnSprites = [Sprite.loadByName("churn")];
waveSprites = ["wave", "wave1"].map(function(name) {
  return Sprite.loadByName(name);
});
setUpGame = function() {
  var box, destruction, player, water;
  player = engine.add({
    "class": "Player",
    x: 0,
    y: 0
  });
  box = engine.add({
    "class": "Rock",
    x: 60,
    y: 180
  });
  (4).times(function(n) {
    return engine.add({
      "class": "Cloud",
      x: n * 128
    });
  });
  water = engine.add({
    color: "blue",
    water: true,
    x: 0,
    y: 160,
    width: App.width + 64,
    height: App.height,
    zIndex: 0
  });
  destruction = engine.add({
    color: "red",
    destruction: true,
    x: -240,
    y: 0,
    width: 10,
    height: App.height,
    zIndex: 7
  });
  destruction.bind("update", function() {
    destruction.I.x += 2 + destruction.I.age / 175;
    return destruction.I.x = destruction.I.x.clamp(player.I.x - 4 * App.width, Infinity);
  });
  destruction.bind("draw", function(canvas) {
    waveSprites.wrap((destruction.I.age / 8).floor()).fill(canvas, -App.width, 0, App.width + 16, App.height);
    return churnSprites.wrap((destruction.I.age / 8).floor()).fill(canvas, 0, 0, 32, App.height);
  });
  water.bind("update", function() {
    var amplitude;
    water.I.x = player.I.x - App.width / 2 - 32;
    amplitude = 15 + water.I.age / 30;
    if (rand(3) === 0 && water.I.age.mod(90) === 0) {
      Sound.play("wave");
    }
    return water.I.y = 160 + amplitude * Math.sin(Math.TAU / 120 * water.I.age);
  });
  return water.bind("draw", function(canvas) {
    return canvas.withTransform(Matrix.translation(-player.I.x.mod(32), 0), function() {
      return depthsSprites.wrap((water.I.age / 8).floor()).fill(canvas, 0, App.height / 2, water.I.width, App.height);
    });
  });
};
setUpGame();
clock = 0;
engine.bind("update", function() {
  var player;
  clock += 1;
  if (player = engine.find("Player").first()) {
    if (clock % 30 === 0) {
      engine.add({
        "class": "Rock",
        x: player.I.x + 2 * App.width
      });
    }
    if (clock % 55 === 0) {
      return engine.add({
        "class": "Cloud",
        x: player.I.x + 2 * App.width
      });
    }
  }
});
restartGame = function() {
  var doRestart;
  doRestart = function() {
    engine.I.objects.clear();
    engine.unbind("afterUpdate", doRestart);
    return setUpGame();
  };
  return engine.bind("afterUpdate", doRestart);
};
engine.bind("afterUpdate", function() {
  var player;
  if (player = engine.find("Player").first()) {
    return engine.I.cameraTransform = Matrix.translation(App.width / 2 - player.I.x, App.height / 2 - player.I.y);
  }
});
engine.bind("draw", function(canvas) {
  XNA_Canvas.fillText("test", 50, 50, 255, 255, 255, 1)
  if (DEBUG_DRAW) {
    return engine.find("Player, Rock").invoke("trigger", "drawDebug", canvas);
  }
});
engine.bind("restart", function() {
  return restartGame();
});
Music.play("SurfN-2-Sur5");

;
