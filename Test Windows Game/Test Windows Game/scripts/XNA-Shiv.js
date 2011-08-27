;
;
var Canvas;
Canvas = function() {
  var currentTransform, fillColor, transformStack;
  transformStack = [];
  currentTransform = Matrix.IDENTITY;
  fillColor = Color("black");

  return {
    clear: function() {
      return XNA_Canvas.clear();
    },
    createPattern: function() {},
    fill: function(color) {
	  log("called fill: color = " + color);
      var a, b, g, r;
      if (color) {
        this.fillColor(color);
      }

	  log(fillColor.toString())
      r = fillColor.r() & 0xFF;
      g = fillColor.g() & 0xFF;
      b = fillColor.b() & 0xFF;
      a = (fillColor.a() * 0xFF) & 0xFF;

      XNA_Canvas.fill(r, g, b, a);
    },
    fillColor: function(color) {
      if (color) {
        fillColor = Color(color);
        return this;
      } else {
        return fillColor;
      }
    },
    fillRect: function(x, y, width, height) {
      var a, b, g, r, _ref;
      r = fillColor.r() & 0xFF;
      g = fillColor.g() & 0xFF;
      b = fillColor.b() & 0xFF;
      a = (fillColor.a() * 0xFF) & 0xFF;
      _ref = currentTransform.transformPoint(Point(x, y)), x = _ref.x, y = _ref.y;
      return XNA_Canvas.fillRect(x, y, width, height, r, g, b, a);
    },
    fillTiledRect: function(image, sourceX, sourceY, sourceWidth, sourceHeight, destX, destY, destWidth, destHeight) {
      var x, y, _ref;
      _ref = currentTransform.transformPoint(Point(destX, destY)), x = _ref.x, y = _ref.y;
      XNA_Canvas.fillTiledRect(image, sourceX, sourceY, sourceWidth, sourceHeight, x, y, destWidth, destHeight);
      return this;
    },
    drawImage: function(image, sourceX, sourceY, sourceWidth, sourceHeight, destX, destY, destWidth, destHeight) {
      var x, y, _ref;
      _ref = currentTransform.transformPoint(Point(destX, destY)), x = _ref.x, y = _ref.y;
      XNA_Canvas.drawImage(image, sourceX, sourceY, sourceWidth, sourceHeight, x, y, destWidth, destHeight);
      return this;
    },
    withTransform: function(matrix, block) {
      currentTransform = currentTransform.concat(matrix);
      transformStack.push(currentTransform);
      try {
        block(this);
      } finally {
        transformStack.pop();
        currentTransform = transformStack.last() || Matrix.IDENTITY;
      }
      return this;
    }
  };
};;
var keydown;
keydown = {};
keydown.__defineGetter__("left", function() {
  return XNA_Keyboard.keyDown("Left");
});
keydown.__defineGetter__("right", function() {
  return XNA_Keyboard.keyDown("Right");
});
keydown.__defineGetter__("space", function() {
  return XNA_Keyboard.keyDown("Space");
});;
Sprite.loadByName = function(name, callback) {
  var img;
  img = XNA_Sprite.loadByName(name);
  return Sprite(img, 0, 0, img.Width, img.Height);
};;
;