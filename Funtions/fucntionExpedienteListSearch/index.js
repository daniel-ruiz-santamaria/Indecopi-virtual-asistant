const { DefaultAzureCredential } = require("@azure/identity")
const { SecretClient } = require("@azure/keyvault-secrets")

const keyVaultName = process.env["KEY_VAULT_NAME"];
const keyVaultUri = `https://${keyVaultName}.vault.azure.net`;
const credential = new DefaultAzureCredential();
const secretClient = new SecretClient(keyVaultUri, credential);

module.exports = async function (context, req) {


    const docNumber = req.query.docNumber;
    const name = req.query.name;
    const year = req.query.year;

    context.log('Function request for search a list of expedient. docNumber: ' + docNumber + ', name: ' + name+ ', year: ' + year);

    context.res = {};
    if ((!docNumber || docNumber==="") || (!name || name==="") || (!year || year==="")) {
        context.res = {
            // status: 200,  Defaults to 200 
            body: "Params docNumber, name and year are required",
            status: 400
        };
    } else {
        try {
            let response_body = await do_async_request(JSON.stringify(
                { 
                    "vcNroDoc": docNumber,
                    "vcNombresRazonSocial": name,
                    "nuAnio": year
                }));
            // holds response from server that is passed when Promise is resolved
            console.log('response',response_body);
            context.res = {
                // status: 200, Defaults to 200 
                body: response_body,
                status: 200
            };
        }
        catch(error) {
            // Promise rejected
            console.log('error',error);
            context.res = {
                // status: 200,  Defaults to 200 
                body: error,
                status: 500
            };
        }
    }
}

async function getSecretFromKV(key) {
    try {
        const secret = await secretClient.getSecret(key);
        return secret.value;
    } catch (error) {
        return null;
    }
}

async function do_async_request(requestData) {
    var hostname = await getSecretFromKV('expedient-search-list-hostname');
    var path = await getSecretFromKV('expedient-search-list-path2');
    var port = await getSecretFromKV('expedient-search-list-port');
    return new Promise((resolve, reject) => { 
        var http = require('https');
        /*
        var options = {
            "method": "POST",
            "hostname": hostname,
            "port": port,
            "path": path,
            "headers": {
              "content-type": "application/json",
              "cache-control": "no-cache",
            }
          };*/

          var options = {
            "method": "POST",
            "hostname": "beta2.indecopi.gob.pe",
            "port": null,
            "path": "/appDSDExpConsApi/chatbot/chatbotListarExpedientes",
            "headers": {
              "content-type": "application/json",
              "cache-control": "no-cache"
            }
          };
          
        var req = http.request(options, function (res) {
            var chunks = [];
            
            res.on("data", function (chunk) {
                chunks.push(chunk);
            });
            
            res.on("end", function () {
                var body = Buffer.concat(chunks);

                var jsonBody = JSON.parse(body.toString());
                var response = []
                if (jsonBody['lstExpediente']) {
                    for (var i = 0 ; i < jsonBody['lstExpediente'].length ; i++) {
                        response.push(
                            {
                                "expedientNumber" : jsonBody['lstExpediente'][i]["vcExpediente"],
                                "requestType" : jsonBody['lstExpediente'][i]["vcTipoSolicitud"],
                                "signType" : jsonBody['lstExpediente'][i]["vcTipoSigno"],
                                "text" : jsonBody['lstExpediente'][i]["vcSegundoNivel"]
                            }
                        );
                    }
                }
                console.log(body.toString());
                resolve(response);
            });
        });

        req.on('error', (error) => {
            reject(error);
        });
          
        req.write(requestData);
        req.end();
    });
}