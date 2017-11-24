const fs = require('fs');
const path = require('path');
const target = './from/kr.pck';
const INIT_DIR = './target'
const lineReader = require('line-reader');
const spreadsheet = require('google-spreadsheet');
const creds = require('./client_secret.json');
const doc = new spreadsheet('1EFyxL44jfltcd5MfRJUgzRZiz7abVOD_3IH5iyx4hqY');
const included = [ /\.js$/, /\.cshtml$/ ];
const regx = /Basis.Parse.ToLanguage\("([^")]+)"\)/g;
var data = {}
var unlisted = {}
const web = require('./data-compare.json');

function includes (p){
  return included.some((v) => p.toString().match(v));
}

function eachFileNameHandler (p) {
  return new Promise((res) => {
    fs.stat(p, function (err, stats) {
      if ( stats.isFile() ) {
        if (includes(p) ) {
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
//directory 내부의 파일 내에서 regex 검사
function merge(to,from) {
  for (var i in to) {
    to[i] = from[i] || to[i]
  }
  return to;
}

function readFile (p) {
  var _filename = p.split(/\/+|\\+/g).slice(-2).join('/');
  return new Promise((resolve, reject) => {
    fs.readFile(p, 'utf-8', function (err, content) {
      if ( content ) {
        content.replace(regx, function (match, str) {
          data[ str ] =  data[ str ] || { map:[], cn : str, kr: '', key:'' };
          if(web [ str ]) {
            data[ str ] = merge(data[ str ], web[str]);
          }
          data[ str ][ 'map' ].push(_filename);
        })
      }
      //file의 경우 무조건 false 로 directory는 path로 resolve 시킴
      resolve(false);
    })
  })
}

//kr파일 읽어서 필요한 형태 확인
function readFromDictionary () {
  return new Promise((resolve) => {
    lineReader.eachLine(target, function (line, last) {
      var _line = line.trim();
      if ( _line.length && _line.split(':').length >= 2 && !_line.match(/^--/) ) {
        var cn = _line.split(':')[ 0 ];
        var kr = _line.split(':')[ 1 ];
        if(data[cn] && data[cn].kr === '') {
          data[cn].kr = kr
        }
      }
      if ( last ) {
        resolve('hello');
      }
    });
  })
}

//directory안에서 실제 쓰이는 내용을 찾기 위해 directory 검사
function readFromDir () {
  return new Promise((resolve, reject) => {
    readDir(INIT_DIR)
      .then((res) => {
        resolve(res)
      })
  })
}

//구글시트로 옮기기
function toGoogleSheet (dic) {
  return new Promise((resolve) => {
    doc.useServiceAccountAuth(creds, function (err) {
      let keys = Object.keys(dic);
      addRow(dic, keys, 0).then(() => {
        resolve('finished');
      });
    })
  })
}

function addRow (dic,keys, n) {
  return new Promise((resolve, reject) => {
    doc.addRow(1, dic[ keys[ n ] ], function (err) {
      if ( err ) {
        return
      }
      if ( n < keys.length ) {
        resolve(addRow(dic, keys, n + 1));
      } else {
        resolve()
      }
    });
  })
}


// console.log(korean);

function run () {
  //dictionary 부터 읽기
  readFromDir()
  //실제 사용된 내용 보기
    .then(() => {
      return readFromDictionary()
    })
    .then(() => {
    //console.log(data);
    return toGoogleSheet(data);
    })
}

run();