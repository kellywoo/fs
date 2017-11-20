const fs = require('fs');
const path = require('path');
const spreadsheet = require('google-spreadsheet');
const gsjson = require('google-spreadsheet-to-json')
const creds = require('./client_secret.json');
const doc = new spreadsheet('1CstBjsjIxj02LwoG0Bx63jg6sFNctZzM9c3-9--Zu7I');


function toGoogleSheet (dic) {
  return new Promise((resolve) => {
    gsjson({
      spreadsheetId: '1CstBjsjIxj02LwoG0Bx63jg6sFNctZzM9c3-9--Zu7I',
      hash: 'key',
      exclude: ['key'],
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