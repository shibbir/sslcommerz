## SSLCommerz
> SSLCommerz integration in ASP.NET Core

[![Build Status](https://travis-ci.com/shibbir/sslcommerz.svg?branch=master)](https://travis-ci.com/shibbir/sslcommerz)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](http://opensource.org/licenses/MIT)

## Instructions
You need to create a free sandbox account from [SSLCommerz](https://developer.sslcommerz.com/registration/). After signing up you will get your store id and store password via email.

In this demo project I've only used the parameters that are required to work with SSLCommerz api. For more details please read the official documentation from [here](https://developer.sslcommerz.com/doc/v4/).

## Configuring environment variables
> Open `launchSettings.json` file inside the *Properties* directory. Then add your store id and store password from SSLCommerz.

```json
"environmentVariables": {
    "StoreId": "your_store_id",
    "StorePassword": "your_store_password"
}
```

## Docker

```bash
docker build -t sslcommerz-aspnetcore .
docker run -d --rm -p 8080:80 --name sslcommerz -e StoreId='<your_store_id>' -e StorePassword='your_store_password' sslcommerz-aspnetcore
```

## License
<a href="https://opensource.org/licenses/MIT">The MIT License</a>
