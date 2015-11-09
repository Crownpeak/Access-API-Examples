//*************************************************************************************
//configuration: modify config.json with the proper values {"instance": "instance name here", "server":  "https://domain name here.com","accessKey": "app key here","username": "machine-machine username here"}
//usage: node syncFolder.js .\Source-Folder '/DevSite/sync-root'
//todo: need to implment gzip support
//todo: implement -recurse and -whatif arguments
//todo: add HTTP 429 rate limit retry-after
//todo: add 
//*************************************************************************************
var colors = require('colors');
var fs = require('fs');
var async = require('async');
var path = require('path');
var accessAPI = require('./accessAPIModule.js');
var files = []; //will hold list of local files that we want to upload
var folderCache = []; //used to cache folder id's to prevent repetitive/costly calls.
var folderType = 4;

//Process command line arguments
var args = process.argv.slice(2);
if (args.length < 2) {
  console.log('Usage: '.yellow.bold + "node app.js [localFolder] [cmsFolder]".red.bold)
  return;
}
var sourceFolder = args.shift(); // local folder that you want to upload to the CMS
var cmsFolder = args.shift(); //destination CMS path, all files and folders from the sourceFolder will be placed under this folder.

console.log("Sync: ".yellow.bold + sourceFolder.green + " => ".yellow.bold + cmsFolder.green);

//check if sourceFolder exists
if (!fs.statSync(sourceFolder).isDirectory()) {
  return console.log(('Could not find source folder: ' + sourceFolder).red.bold)
}

sourceFolder = path.resolve(sourceFolder);  //get full pathname to make our life easier later, with string replace.

/************** main logic starting **********************/
//make call to authenticate, for this example, username and password will be looked up by the accessAPI library
accessAPI.auth( function (data) {
  //authenticated
  if (data.resultCode != 'conWS_Success')
  {
    console.log('Unable to authenticate: '+ data.errorMessage.red.bold);
  }
  else
  {
    //check if an asset exist  
    accessAPI.AssetExists(cmsFolder, function (data) 
    {
      if (data.exists)
      {
        folderCache[cmsFolder] = data.assetId;
        //auth succeeded, and destination folder exists, now start uploading files/folders
        console.log('sync folder now...'.green.bold + ' id: ' + data.assetId);
        buildFolderList( sourceFolder, function (data)
        {
          //now sync, by iterating through files
          async.eachSeries(files, fileIterator, function (err, destdir) {
            if (err) console.log(('error: '+err).red.bold);
            console.log('all done!');
            console.log('logging out');
            //accessAPI.logout();
            console.log('logged out');
          });
        });
      }
      else 
      {
        console.log(('Folder does not exist: ' + cmsFolder).red.bold)
        console .log(data);
      }
    });
  }
}, (function (err) {
    console.log(err.red.bold);
}));


/************ end of main logic **********/

//checks if a folder exists in the cms, creates it, if any part of it is missing.
function checkFolderList(destdir, callback) {
  var list = destdir.replace(cmsFolder,'').split('/');
  var basePath = cmsFolder;
  async.eachSeries(list, function (part, callbackDone)
  {
    if (part.length>0)
    {
      var parentFolder = basePath;
      var parentFolderId = folderCache[basePath];
      basePath = basePath +'/'+ part;
      if (folderCache[basePath] != undefined)
      {
        callbackDone(); //folder is already cached
      }
      else
      {
        accessAPI.AssetExists(basePath, function (data)
        {
          if (data.exists)
          {
            folderCache[basePath] = data.assetId;  
            return callbackDone(); //folder was not cached, but we looked it up and cached it
          }
          else
          {
            accessAPI.AssetCreate(part, parentFolderId, -1, folderType, -1, -1, -1, function(data) 
            {
              //note: AssetCreate returns an asset object
              if (data.asset.id > 0)
              {
                console.log('Created folder = ' + part + ' in ' + parentFolderId + ' new id: '+ data.asset.id);
                folderCache[basePath] = data.asset.id;
                return callbackDone(); //part of folder was missing, we created it and chached it
              }
              else
              {
                var msg = 'Unable to create folder = ' + part + ' in ' + parentFolderId + ' code: ' + data.resultCode + ' errorMessage: ' + data.errorMessage;
                console.log(msg.red.bold);
                return callbackDone(msg);
              }
            });
          }
        });
      }
    }
    else
    {
      return callbackDone();
    }
  }, function (err)
  {
    if (err) console.log(('checkFolderList error: ' + err).red.bold);
    callback(err, folderCache[basePath]);
  });
}

//iterator for files, checks if folder exists, creates any that are missing, and then uploads file
function fileIterator (file, callbackDone) {
  var fn = path.basename(file); //filename from full path
  var dir = path.dirname(file).replace(sourceFolder, '');  //folder part only
  var destdir = cmsFolder+dir.split('\\').join("/");  //calculate cms destination folder location
  var destfile = destdir + '/'+fn; //full cms destination path and name

  checkFolderList(destdir, function (err, folderId)
  {
     accessAPI.AssetExists(destfile, function (data) 
     {
        if (!data.exists)
        {
          if (folderId < 1) callback('error folderid < 1'+folderId);
          console.log('upload: '.red.bold + destfile + ' in folder id: '+ folderId);
          var rawBytes = fs.readFileSync( file );
          var bytes = new Buffer(rawBytes).toString('base64');
          accessAPI.AssetUpload(fn, folderId, -1, -1, bytes, function(data)
          {
            var err = null;            
            if (data.resultCode != 'conWS_Success') { err = data.resultCode}
            callbackDone(err, destfile);
          });
        }
        else
        {
          console.log(('skipping file: ' + destfile).yellow.bold);
          callbackDone(null, destdir);
        }
    });
  });  
}

//read local folder, and process all files/folders in it
function buildFolderList(dir, callback) {
  //console.log('getting local files: ' + dir);
  fs.readdir(dir, function(err, list) {
    if (err) return callback(err);
    if (list.length == 0) return callback('no more files in folder');
    //console.log('found local files: ' + files.length);
    async.each(list, function(file, done) {
      var fpath = path.join(dir,file);
      fs.stat(fpath, function(err, stat) {
        if (err) return done(err);
        if (stat.isDirectory()) return buildFolderList(fpath, done);
        files.push(fpath);
        done();
      });
    }, function(err) {
        // called when all done or error
        callback(err);
    })
 
  });
}