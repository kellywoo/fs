const fs = require('fs');
const path = require('path');
const INIT_DIR = './target'
const included = [ /\.cshtml$/ ];
var data = {}
const korean = require('./data.json');

function includes (p) {
  return included.some((v) => p.toString().match(v));
}

function eachFileNameHandler (p) {
  return new Promise((res) => {
    fs.stat(p, function (err, stats) {
      if ( stats.isFile() ) {
        if ( includes(p) ) {
          changeFile(p, data)
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

function changeFile (p) {
  return new Promise((resolve, reject) => {
    var dist = p.replace('target', 'dist');
console.log(dist)
    fs.readFile(p, 'utf-8', function (err, content) {
      if ( content ) {
        var reg;
        var _data = content;
        console.log(_data)
        for ( var cn in korean ) {
          reg = new RegExp('Basis.Parse.ToLanguage\\("(' + cn.replace(/:\(/g,':\\(') + ')"\\)', "g")
          _data = _data.replace(reg, function (match, p) {
            return 'Basis.Parse.Localization("' + korean[cn].key + '")'
          })
        }
        fs.writeFile(dist, _data, 'utf-8', function (err) {
          if ( err ) {
            console.log(err);
          }
          resolve(false);
        })
      } else {
        resolve(false);
      }
    })
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

// console.log(korean);

  function run () {
    //dictionary 부터 읽기
    readFromDir()
    //실제 사용된 내용 보기
      .then(() => {
        console.log('finished');
      })

  }

  run();