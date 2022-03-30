const { DefaultAzureCredential } = require("@azure/identity")
const { SecretClient } = require("@azure/keyvault-secrets")

const keyVaultName = process.env["KEY_VAULT_NAME"];
const keyVaultUri = `https://${keyVaultName}.vault.azure.net`;
const credential = new DefaultAzureCredential();
const secretClient = new SecretClient(keyVaultUri, credential);

module.exports = async function (context, req) {

    const docNumber = req.query.docNumber;
    const name = req.query.name;
    const expNumber = req.query.expNumber;

    context.log('Function request for search a expedient. docNumber: ' + docNumber + ', name: ' + name+ ', expNumber: ' + expNumber);

    context.res = {};
    if ((!docNumber || docNumber==="") || (!name || name==="") || (!expNumber || expNumber==="")) {
        context.res = {
            // status: 200, /* Defaults to 200 */
            body: "Params docNumber, name and expNumber are required",
            status: 400
        };
    } else {
        try {
            let response_body = await do_async_request(JSON.stringify(
                { 
                    vcNroDocumento: docNumber,
                    vcNombresRazonSocial: name,
                    vcNroExpediente: expNumber
                }));
            // holds response from server that is passed when Promise is resolved
            console.log('response',response_body);
            context.res = {
                // status: 200, /* Defaults to 200 */
                body: response_body,
                status: 200
            };
        }
        catch(error) {
            // Promise rejected
            console.log('error',error);
            context.res = {
                // status: 200, /* Defaults to 200 */
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
    var hostname = await getSecretFromKV('expedient-search-hostname');
    var path = await getSecretFromKV('expedient-search-path');
    var port = await getSecretFromKV('expedient-search-port');
    return new Promise((resolve, reject) => { 
        var http = require('https');
        var options = {
            "method": "POST",
            "hostname": hostname,
            "port": port,
            "path": path,
            "headers": {
              "content-type": "application/json",
              "cache-control": "no-cache",
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
                var response = {
                    "state": jsonBody['objRespuesta']['vcTexto1'],
                    "term": jsonBody['objRespuesta']['vcTexto2']
                };
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