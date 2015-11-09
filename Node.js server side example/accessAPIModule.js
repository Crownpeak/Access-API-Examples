var requestify = require('requestify');
var prompt = require('sync-prompt').prompt;
var colors = require('colors');
var fs = require('fs');

var apikey = "";
var domain = '';
var instance = '';
var cookies = null;
var username = '';
var password = '';

//read a json file with configuration fields, so we don't have to hard code them
if (fs.existsSync('config.json')) {
  console.log('reading config.json'.yellow.bold);
  var config = JSON.parse(fs.readFileSync('config.json', { "encoding": "utf8" }));
  instance = config.instance;
  domain = config.server;
  apikey = config.accessKey;
  username = config.username;
  console.log('instance: ' + config.instance);
  console.log('domain: ' + config.server);
  console.log('username: ' + config.username);
}

var hidden = true;
//prompt user for password
if (password.length == 0) password = prompt('Password: ', hidden);

var baseURL = domain + '/' + instance + '/cpt_webservice/accessapi';

//setup http headers
var options = {
  body: '',
  method: 'POST',
  headers: {
    'x-api-key': apikey,
    'Content-Type': 'application/json; charset=utf8',
    //'Accept-Encoding': 'gzip, deflate '
  }
};

exports.auth = function (callback) {
  var body = { "instance": instance, "username": username, "password": password, "remember_me": false, "timeZoneOffsetMinutes": -480 };
  return restPost('/auth/authenticate', body, callback);
}

exports.logout = function (callback) {  
  return restPost('/auth/logout', null, callback);
}

exports.AssetExists = function (path, callback) {
  var body = { "assetIdOrPath": path };
  return restPost('/asset/Exists', body, callback);
}

exports.AssetUpload = function (newName, folderId, modelId, workflowId, bytes, callback) {
  var body = { "newName": newName,
               "destinationFolderId": folderId,
               "modelId": modelId,
               "workflowId": workflowId,
               "bytes": bytes };
  if (folderId == 0 || folderId == undefined) return callback('not allowed to import to root');               
  return restPost('/asset/Upload', body, callback);
}

exports.AssetCreate = function (newName, folderId, modelId, type, devTemplateLanguage, templateId, workflowId, callback) {
  var body = { "newName": newName,
  "destinationFolderId": folderId,
  "modelId": modelId,
  "type": type,
  "devTemplateLanguage": devTemplateLanguage,
  "templateId": templateId,
  "workflowId": workflowId }  
  if (folderId == 0 || folderId == undefined)
  {
    console.log('create asset error, folderId = ' + folderId);
    return callback('not allowed to import to root');  
  } 
  return restPost('/asset/Create', body, callback);
}

//main http call
function restPost(url, body, callback) {
  url = baseURL + url;
  console.log("calling: ".yellow.bold + url.green.bold);
  options.body = body;
  //check if we need pass the cookies
  if (cookies != null) {
    // todo: try to reuse headers from above.
    options.headers = {
      'x-api-key': apikey,
      'Content-Type': 'application/json; charset=utf8',
      'Cookie': cookies
      //'Accept-Encoding': 'gzip, deflate '
    }
  }

  requestify.request(url, options).then(function (resp) {
      processCookies(resp);
      callback(JSON.parse(resp.body));
    }, function (err) {
      //todo: handle http 429, rate limiting busy, retry-after
      callback(JSON.parse(err.body));
    });
}

//handles cookies between http calls
function processCookies(resp, callback) {
  if (resp.headers['set-cookie'] != null) {
    var cooks = resp.headers['set-cookie'];
    var newCookies = '';
    var cookieCount = 0;
    cooks.forEach(function (cookie) {
      var parts = cookie.split(';');
      //console.log(parts);
      if (cookieCount++ > 0) {
        newCookies += "; ";
      }
      newCookies += parts[0];
    });
    cookies = newCookies;
  }
}
