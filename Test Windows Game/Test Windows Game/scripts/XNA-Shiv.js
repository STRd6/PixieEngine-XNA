;
;
var Canvas;
Canvas = function() {
  var currentTransform, transformStack;
  transformStack = [];
  currentTransform = Matrix.IDENTITY;
  return {
    drawImage: function(image, sourceX, sourceY, sourceWidth, sourceHeight, destX, destY, destWidth, destHeight) {
      var x, y, _ref;
      _ref = currentTransform.transformPoint(Point(destX, destY)), x = _ref.x, y = _ref.y;
      __XNA__Canvas.drawImage(image, sourceX, sourceY, sourceWidth, sourceHeight, x, y, destWidth, destHeight);
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
};
Sprite.loadByName = function(name, callback) {
  var img;
  img = __XNA__Sprite.loadByName(name);
  return Sprite(img, 0, 0, img.Width, img.Height);
};;
;