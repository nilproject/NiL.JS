/**
 * oop.js - Pseudo-object-oriented library for JavaScript
 * @module OOP
 * @class OOP
 * @author Drew Ewing
 * @beta
 **/
var OOP = function () {
  /**
   * The OOP configuration object
   * @prop config
   **/
  this.config = {
    debug: false,
    debugLevel: 1,
    baseURL: '/'
  };
  /**
   * Debug to the console
   * @method debug
   * @param {mixed} arguments - Anything you want to debug
   * @example
   *      $oop.debug('Testing 1, 2, 3', someObject, someMethod);
   */
  this.debug = function () {
    if ($oop.config.debug && typeof(console !== 'undefined') && typeof(console.log !== 'undefined')) {
      try {
        console.log('[OOPJS]', arguments);
      } catch (e) {
      }
    }
  };
  /**
   * Create a new object from a registered class definition
   * @method new
   * @param name - A registered class name
   * @returns {OOP.Object} - Returns a newly instantiated object
   * @constructor
   * @example
   *      var object = $oop.new('MyClass');
   **/
  this.new = function (name) {
    var classDef = $oop.getClass(name);
    var object = new OOP.Object();
    // Set object name
    object._name = name;
    // Extract the ancestors array
    classDef._ancestors = $oop._extractAncestors(classDef._ancestors);
    if (classDef._ancestors.length > 0) {
      for (var x in classDef._ancestors) {
        var ancestor = $oop.getClass(classDef._ancestors[x]);
        object = $oop._extend(object, ancestor._userland);
        for (var y in classDef._ancestors) {
          // Make the class definition aware of all ancestors
          object._ancestors.push(classDef._ancestors[y]);
        }
      }
    }
    // Extend the object from the class's defined userland
    object = $oop._extend(object, classDef._userland);
    return (function (obj) {
      return function () {
        if (typeof(obj.construct) !== 'undefined') {
          obj.construct.apply(obj, arguments);
        }
        return obj;
      };
    })(object);
  };
  /**
   * Recursively extract ancestors
   * @param ancestors
   * @returns {Array} - An array of ancestors
   * @private
   */
  this._extractAncestors = function (ancestors) {
    ancestors = ancestors || [];
    for (var x in ancestors) {
      var ancestor = $oop.getClass(ancestors[x]);
      var ancestorsAppend = $oop._extractAncestors(ancestor._ancestors);
      for (var y in ancestorsAppend) {
        ancestors.push(ancestorsAppend[y]);
      }
    }
    return ancestors;
  };
  /**
   * The registry of class definitions
   * @prop _classRegistry
   **/
  this._classRegistry = {};
  /**
   * Retrieve a {{#crossLink "OOP.Class"}}class definition{{/crossLink}} given a class name
   * @method getClass
   * @param name - The class name
   * @returns {OOP.Class} - The {{#crossLink "OOP.Class"}}class definition{{/crossLink}} of the given class
   * @example
   *      var classDef = $oop.getClass('MyClass')
   */
  this.getClass = function (name) {
    var classDef = this._classRegistry[name];
    if (typeof(classDef) === 'undefined') {
      throw 'Class "' + name + '" is undefined';
    }
    return classDef;
  };
  /**
   * Execute the provided method when the OOP service is waiting
   * @method ready
   * @param method - A method to execute when the OOP environment is ready
   * @example
   *      $oop.ready(function() { console.log("I'm ready!"); });
   */
  this.ready = function (method) {
    $oop._onReady.push(method);
    if ($oop._ready) {
      for (var x in $oop._onReady) {
        $oop._onReady[x]();
      }
      $oop._onReady = [];
    }
  };
  /**
   * Look for needle in haystack
   * @method contains
   * @param needle - The value to look for
   * @param haystack - An array or object to look in
   * @returns {boolean} - True/False
   * @example
   *      if($oop.contains(needle, haystack)) { doSomething(); }
   */
  this.contains = function (needle, haystack) {
    for (var i in haystack) {
      if (haystack[i] === needle) {
        return true;
      }
    }
    return false;
  };
  /**
   * Duplicate an object
   * @method clone
   * @param source - The function or object to clone
   * @returns {Function|Object} - A duplicate of the provided function or object
   * @example
   *      var cloned = $oop.clone(object);
   */
  this.clone = function (source) {
    var cloned;
    if (typeof(source) === 'function') {
      cloned = function () {
      };
      var dupe = new source();
      for (var x in dupe) {
        cloned[x] = dupe[x];
      }
      for (var y in dupe.prototype) {
        cloned[y] = dupe[y];
      }
    } else {
      cloned = {};
      if (typeof(source) === 'object') {
        for (var z in source) {
          cloned[z] = source[z];
        }
      }
    }
    return cloned;
  };
  /**
   * Register a function to be called when a matching {{#crossLink "OOP.hook"}}hook event{{/crossLink}} occurs
   * @method onHook
   * @param hook - The hook to listen for
   * @param method - The method to call when hooked
   * @example
   *      $oop.onHook('login', function() { doSomething(); });
   */
  this.onHook = function (hook, method) {
    if (typeof($oop._hooks[hook]) === 'undefined') {
      $oop._hooks[hook] = [];
    }
    if (typeof(method) === 'function') {
      $oop._hooks[hook].push(method);
    } else {
      throw "Hook method must be a function";
    }
  };
  /**
   * Hook the given string, calling matching registered {{#crossLink "OOP.onHook"}}onHook{{/crossLink}} events
   * @method hook
   * @param hook - The string to hook
   * @param data - The data provided to matching {onHook} events
   * @returns {*} - The data is sequentially passed to and returned by onHook events, and the result is returned
   * @example
   *      $oop.hook('login', userData);
   */
  this.hook = function (hook, data) {
    if (typeof($oop._hooks[hook]) === 'object') {
      for (var i in $oop._hooks[hook]) {
        var method = $oop._hooks[hook][i];
        if (typeof(method) === 'function') {
          var tdata = method(data);
          if (typeof(tdata) !== 'undefined' && tdata.length > 0) {
            data = tdata;
          }
        }
      }
    }
    return data;
  };
  /**
   * Creates a new Object and appends to it the methods and properties from a and b
   * @method _extend
   * @param a - An object or function
   * @param b - An object or function
   * @returns - An object
   * @example
   *      var mergedObj = $oop._extend(funcA, funcB);
   **/
  this._extend = function (a, b) {
    var c = {};
    for (var w in a) {
      c[w] = a[w];
    }
    for (var x in b) {
      c[x] = b[x];
    }
    for (var y in a.prototype) {
      c[y] = a.prototype[y];
    }
    for (var z in b.prototype) {
      c[z] = b.prototype[z];
    }
    return c;
  };
  /**
   * The OOP service ready state
   * @prop _ready
   */
  this._ready = false;
  /**
   * The qeue of methods to be executed when a ready state is achieved
   * @prop _onReady
   */
  this._onReady = [];
  /**
   * The qeue of methods to be executed when hooks occur
   */
  this._hooks = {};
};
/**
 * The oop.js class definition constructor
 * @namespace OOP
 * @class Class
 * @constructor
 *
 * @param {string} name - The name of the class to be registered
 *
 * @prop {string} _name - The name of the class
 * @prop {string} _ancestors - An array of the ancestor classes
 * @prop {object} _userland - The source container for the class
 **/
var global = function () { return this }();
OOP.Class = function (name) {
  this._name = name || 'OOP.Class';
  this._ancestors = [];
  this._userland = {};
  $oop._classRegistry[name] = this;
  // Register the global class proxy
  var proxy = function () {
    return $oop.new(name).apply(this, arguments);
  };
  if(typeof(window) !== 'undefined') {
    window[name] = proxy;
  } else if(typeof(global) !== 'undefined') {
    global[name] = proxy;
  }
  return this;
};
/**
 * Extend another registered class
 *
 * @method extends
 * @chainable
 * @example
 *      OOP.Class('Child').extends('Parent').via(function(){});
 *
 * @param name - The name of the class to extend
 */
OOP.Class.prototype.extends = function (name) {
  this._ancestors.push(name);
  return this;
};

/**
 * Define a function to serve as this class's userland
 *
 * @method via
 * @chainable
 * @example
 *      OOP.Class('Child').extends('Parent').via(function(){});
 *
 * @param method - The userland function
 */
OOP.Class.prototype.via = function (method) {
  this._userland = {};
  var dummy = new method();
  for (var x in dummy) {
    this._userland[x] = dummy[x];
  }
  for (var y in dummy.prototype) {
    this._userland[y] = dummy.prototype[y];
  }
  return this;
};

/**
 * The default object container
 * @namespace OOP
 * @class Object
 * @constructor
 *
 * @property {string} _name     The name of the object
 * @property {array} _ancestors   An array of ancestor class names
 **/
OOP.Object = function () {
  /** The default object properties **/
  this._name = 'OOP.Object';
  this._ancestors = [];
};

/**
 * Retrieve the parent class
 *
 * @method getParent
 * @example
 *      function extendedFunc() { this.getParent().extendedFunc(); somethingElse(); }
 *
 * @return {OOP.Class} The userland object of the parent class
 */
OOP.Object.prototype.getParent = function () {
  return $oop.getClass(this._ancestors[this._ancestors.length])._userland || false;
};

/**
 * Determine if this object is an instance of a given class
 *
 * @method instanceOf
 * @example
 *      if(someObj.instanceOf('someOtherObj')) { doSomething(); }
 *
 * @param name - The class name in question
 *
 * @return {boolean} True or false
 */
OOP.Object.prototype.instanceOf = function (name) {
  /** TODO: Track ancestry **/
  return $oop.contains(name, this._ancestors);
};

/**
 * An alias to {{#crossLink "OOP.Class"}}{{/crossLink}}
 * @return {OOP.Class}
 */
var $class = function (name) {
  return new OOP.Class(name);
};

var $oop = new OOP();

/**
 * An alias to {{#crossLink "OOP.New"}}{{/crossLink}}
 * @return {OOP.New}
 */
var $new = $oop.new;

if(typeof(window) !== 'undefined') {
  window.$oop = $oop;
  window.$new = $new;
  window.$class = $class;

  /**
   * Notifies the OOP service that it is ready
   */
  window.onload = function () {
    $oop._ready = true;
    $oop.debug('Window is ready, processing ready callbacks');
    $oop.ready(function () {
      $oop.debug('OOP is ready');
    });
  };
} else {
  if(typeof(global) !== 'undefined') {
    global.$oop = $oop;
    global.$new = $new;
    global.$class = $class;
  }
  if(typeof(module) !== 'undefined') {
    module.exports = $oop;
  }
  $oop._ready = true;
  $oop.ready(function () {
    $oop.debug('OOP is ready');
  });
}