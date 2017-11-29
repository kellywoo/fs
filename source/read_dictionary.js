const fs = require('fs');
const path = require('path');
const target = './from/kr.pck';
const INIT_DIR = './target'
const lineReader = require('line-reader');
const spreadsheet = require('google-spreadsheet');
const creds = require('./client_secret.json');
const doc = new spreadsheet('1v383oZPJLA216aCYnlP4GIAzJIXtSWjLP8wEQsbqf1g');
const included = [ /\.js$/, /\.cshtml$/ ];
const regx = /Basis.Parse.Localization\("([^")]+)"\)/g;
var data = {}
var unlisted = {}

function readFromDictionary () {
  return new Promise((resolve) => {
    lineReader.eachLine(target, function (line, last) {
      var _line = line.trim();
      if ( _line.length && _line.split(':').length >= 2 && !_line.match(/^--/) ) {
        var cn = _line.split(':')[ 0 ];
        var kr = _line.split(':')[ 1 ];
        data[ cn ] = { 'cn': cn, 'kr': kr, 'map': [] }
      }
      if ( last ) {
        resolve('hello');
      }
    });
  })
}

function eachFileNameHandler (p) {
  return new Promise((res) => {
    fs.stat(p, function (err, stats) {
      if ( stats.isFile() ) {
        if ( included.some((v) => p.toString().match(v)) ) {
          readFile(p, data)
            .then(() => {
              res(false)
            })
        } else {
          res(false);
        }
      } else {
        //directory의 경우만 path 값을 넘긴다.
        res(p)
      }
    })
  })
}

function readDir (dirname) {
  return new Promise((resolve, reject) => {
    fs.readdir(dirname, function (err, filenames) {
      if ( err ) throw err;

      Promise.all(filenames.map((file) => {
        return eachFileNameHandler(path.resolve(dirname, file))
      }))
        .then((isDirectory) => {
          return isDirectory.filter(c => !!c)
        })
        .then((dirArray) => {
          return dirArray.reduce(
            (pr, dir) => pr.then(() => readDir(dir))
            , Promise.resolve())
        }).then(() => {
        resolve()
      })
    })
  })
}

function readFile (p) {
  var _filename = p.split(/\/+|\\+/g).slice(-2).join('/');
  return new Promise((resolve, reject) => {
    fs.readFile(p, 'utf-8', function (err, content) {
      if ( content ) {
        content.replace(regx, function (match, str) {
          if ( data[ str ] ) {
            data[ str ][ 'map' ].push(_filename);
          } else {
            unlisted[ str ] = unlisted[ str ] || { map: [] };
            unlisted[ str ][ 'map' ].push(_filename);
          }
        })
      }
      //file의 경우 무조건 false 로 directory는 path로 resolve 시킴
      resolve(false);
    })
  })
}

function readFromDir () {
  return new Promise((resolve, reject) => {
    readDir(INIT_DIR)
      .then((res) => {
        resolve(res)
      })
  })
}

function toGoogleSheet () {
  return new Promise((resolve) => {
    console.log('toGoogleSheet')
    doc.useServiceAccountAuth(creds, function (err) {
      let keys = Object.keys(data);
      addRow(keys, 0).then(() => {
        resolve('finished');
      });
    })
  })
}

function addRow (keys, n) {
  console.log(keys,n);
  return new Promise((resolve, reject) => {
    doc.addRow(1, data[ keys[ n ] ], function (err) {
      if ( err ) {
        return
      }
      if ( n < keys.length ) {
        resolve(addRow(keys, n + 1));
      } else {
        resolve()
      }
    });
  })
}

//dictionary 부터 읽기
readFromDictionary()
//실제 사용된 내용 보기
  .then(() => {
    return readFromDir()
  })
  .then(() => {
    return toGoogleSheet();
  })
  .then((res) => {
    console.log(res);
  })
