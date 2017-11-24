const fs = require('fs');
const spreadsheet = require('google-spreadsheet');
const gsjson = require('google-spreadsheet-to-json')
const creds = require('./client_secret.json');


function toGoogleSheet (dic) {
  return new Promise((resolve) => {
    gsjson({
      spreadsheetId: '1v383oZPJLA216aCYnlP4GIAzJIXtSWjLP8wEQsbqf1g',
      hash: 'key',
      exclude: ['map','desc','key'],
      credentials: creds
    })
      .then(function(result) {
        fs.writeFile('data.json',JSON.stringify(result),'utf-8',function(e){
          if(e){
            console.warn(e);
          }
        })
      })
  })
}

toGoogleSheet();