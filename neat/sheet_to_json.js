const fs = require('fs');
const gsjson = require('google-spreadsheet-to-json');
const path = require('path');

let config = {
  spreadsheetId: '',
  credentials: '',
}

function merge(to,from) {
  if(!from) {
    return to;
  }
  for (var i in from) {
    to[i] = from[i]
  }
  return to;
}

function toGoogleSheet (fileName, option) {
  if(typeof fileName ==='object'){
    option = fileName;
    fileName = null;
  }
  fileName = fileName || 'sheet.json';
  if(typeof option==='object'){
    if(!option.hash) {
      delete config.hash;
    }
    config = merge(config,option);
  }
  return new Promise((resolve) => {
    gsjson(config)
      .then(function(result) {
        fs.writeFile(path.resolve(process.cwd(), fileName).toString(), JSON.stringify(result),'utf-8',function(e){
          if(e){
            console.warn(e);
          }
        })
      })
  })
}

module.exports = toGoogleSheet;