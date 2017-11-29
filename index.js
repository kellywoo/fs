const sheetToJson = require('./neat/sheet_to_json');
const creds = require('./client_secret.json');
let config = {
  spreadsheetId: '1v383oZPJLA216aCYnlP4GIAzJIXtSWjLP8wEQsbqf1g',
  hash: 'key',
  exclude: ['map','desc','key'],
  credentials: creds,
}
//relative path로
sheetToJson(config);