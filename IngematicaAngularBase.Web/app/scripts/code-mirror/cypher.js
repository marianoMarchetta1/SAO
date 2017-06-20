// CodeMirror, copyright (c) by Marijn Haverbeke and others
// Distributed under an MIT license: http://codemirror.net/LICENSE

// By the Neo4j Team and contributors.
// https://github.com/neo4j-contrib/CodeMirror

(function(mod) {
  if (typeof exports == "object" && typeof module == "object") // CommonJS
    mod(require("../../lib/codemirror"));
  else if (typeof define == "function" && define.amd) // AMD
    define(["../../lib/codemirror"], mod);
  else // Plain browser env
    mod(CodeMirror);
})(function(CodeMirror) {
  "use strict";
  var wordRegexp = function(words) {
    return new RegExp("^(?:" + words.join("|") + ")$", "i");
  };

  CodeMirror.defineMode("cypher", function(config) {
    var tokenBase = function(stream/*, state*/) {
      var ch = stream.next();
      if (ch === "\"") {
        stream.match(/.+?["]/);
        return "string";
      }
      if (ch === "[") {
      	stream.match(/.+?[\]]/);
      	return "variable";
      }
      if (operatorChars.test(ch)) {
        stream.eatWhile(operatorChars);
        return null;
      } else {
        stream.eatWhile(/[_\w\d]/);
        var word = stream.current();
        if (funcs.test(word)) return "builtin";
        if (preds.test(word)) return "def";
        if (keywords.test(word)) return "keyword";
        return "comment";
      }
    };
    var pushContext = function(state, type, col) {
      return state.context = {
        prev: state.context,
        indent: state.indent,
        col: col,
        type: type
      };
    };
    var popContext = function(state) {
      state.indent = state.context.indent;
      return state.context = state.context.prev;
    };
    var indentUnit = config.indentUnit;
    var curPunc;
    var funcs = wordRegexp([]);
    var preds = wordRegexp(["and", "or"]);
    var keywords = wordRegexp([]);
    var operatorChars = /[*+\-<>=!^]/;

    return {
      startState: function(/*base*/) {
        return {
          tokenize: tokenBase,
          context: null,
          indent: 0,
          col: 0
        };
      },
      token: function(stream, state) {
        if (stream.sol()) {
          if (state.context && (state.context.align == null)) {
            state.context.align = false;
          }
          state.indent = stream.indentation();
        }
        if (stream.eatSpace()) {
          return null;
        }
        var style = state.tokenize(stream, state);
        if (style !== "comment" && state.context && (state.context.align == null) && state.context.type !== "pattern") {
          state.context.align = true;
        }
        
        return style;
      }
    };
  });

  CodeMirror.defineMIME("application/x-cypher-query", "cypher");

});
