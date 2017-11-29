let config = {
  regx:/Basis.Parse.Localization\("([^")]+)"\)/g,
  included : [ /\.cs$/],
  source:'./source/Controllers'
}
const fs = require('fs');
const path = require('path');
let data = {}

var readFileHandler = function(content, regx){
  console.log('readFileHandler')
  content.replace(regx, function (match, str) {
    console.log(data)
    data[ str ] =  0;
  })
}

function includes (p){
  if(!config.included || (Array.isArray(config.included) && !config.included.length)) {
    return true;
  } else {
    return config.included.some((v) => p.toString().match(v));
  }
}

function eachFileNameHandler (p) {
  return new Promise((res) => {
    fs.stat(p, function (err, stats) {
      if ( stats.isFile() ) {
        if (includes(p)) {
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

function readFile (p) {
  var _filename = p.split(/\/+|\\+/g).slice(-2).join('/');
  console.log(_filename);
  return new Promise((resolve, reject) => {
    fs.readFile(p, 'utf-8', function (err, content) {
      if ( content ) {
        readFileHandler(content, config.regx)
      }
      //file의 경우 무조건 false 로 directory는 path로 resolve 시킴
      resolve(false);
    })
  })
}


//directory안에서 실제 쓰이는 내용을 찾기 위해 directory 검사
function readFromDir () {
  return new Promise((resolve, reject) => {
    readDir(config.source)
      .then((res) => {
        resolve(res)
      })
  })
}

function merge(to,from) {
  if(!from) {
    return to;
  }
  for (var i in to) {
    to[i] = from[i] || to[i]
  }
  return to;
}

function run (conf) {
  if(conf){
    config = merge(config, conf);
  }

  //dictionary 부터 읽기
  readFromDir()
  //실제 사용된 내용 보기
    .then(() => {
      fs.writeFile('data.json',JSON.stringify(data),'utf-8',function(err){console.log('done')})
    })
}

run();