<p align="center">
<img src="https://raw.githubusercontent.com/SubhenduShekhar/BEx/main/images/logo.PNG" alt="logo"><br/>
</p>


## Content
- [Content](#content)
- [Introduction](#introduction)
- [Project Folder Structure](#project-folder-structure)
- [Initial setup](#initial-setup)
  - [Coded](#coded)
  - [Scriptless](#scriptless)
- [Project Configurations](#project-configurations)
  - [Configuring `FrameworkConfig.json`](#configuring-frameworkconfigjson)
    - [Coded](#coded-1)
    - [Scriptless](#scriptless-1)
  - [Configuring `TestSuite.json`](#configuring-testsuitejson)
    - [Example](#example)
  - [Configuring `Environment.json`](#configuring-environmentjson)
- [Scripting test cases](#scripting-test-cases)
  - [Coded](#coded-2)
  - [Scriptless](#scriptless-2)
- [Scriptless keywords](#scriptless-keywords)
  - [More details](#more-details)
  - [Keywords extensions](#keywords-extensions)
  - [Custom inbuild functions](#custom-inbuild-functions)
  - [Extended Features](#extended-features)
- [Commandline executions](#commandline-options)
  - [--tests=<Comma,Seperated,Tests>](#--testscommaseperatedtests)
  - [--toreport=false](#--toreportfalse)
  - [--update@version](#--updateversion)
  - [--summary](#--summary)
  - [Other options](#other-options)
    - [--version](#version)
- [Slack Notification](#slack-notifications)
- [AWS CodePipeline Integration](#aws-codepipeline-integration)
  - [Project specific customisations for AWS files](#project-specific-customisations-for-aws-files)
- [Docker Integration](#docker-integration)
- [Credits](#credits)

## Introduction
Thank you for using BEx framework for your automation. In the next list of items, you will be walked through
things, which we have felt amazing while developing also.

## Project Folder Structure

├─── BEx<br/>
├   &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;└─── Framework<br/>
├   &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;   └─── Config<br/>
├   &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;└─── `FrameworkConfig.json` and `TestSuite.json`<br/>
├─── Project<br/>
├   &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;└─── TestScript<br/>
├   &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;   └─── `TestCase.json`/`TestCase.cs`<br/>
├   &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;└─── TestData<br/>
├   &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;   └─── `TestData.xlsx`<br/>

## Getting Onboarded
Please follow the below steps to get onboarded to the framework:
1. Create a Console Framework for .NET application
2. Download the latest framework release from framework repository
3. In Project -> Dependencies -> Browser -> Browser
4. Browse `BEx.dll` files and add to your project
5. Configure Framework/Config/FrameworkConfig.json. For more details, click [here](#configuring-frameworkconfigjson)

### Coded
**Suggested IDE : Microsoft Visual Stidio**
1. Add your test data files in `TestData` folder
2. Add your test scripts in `TestScripts` folder. For syntax related information of creating tests, click [here](#test-script-syntax)
3. Build your code from Visual Studio.
4. Run your tests by running `ProjectName.exe` file in your build folder.

### Scriptless
**Suggested IDE : Visual Studio Code**
1. Configure TestSuite.json file. For more details click [here](#configuring-testsuitejson)
2. Add your test case files in `Project/TestScipts/` location.
3. Write your test cases. For more details, click [here](#scriptless-2)
4. Run your tests by running `BEx.exe` file in your build folder.

## Project Configurations

### Configuring `FrameworkConfig.json`

**File name is strict in nature**<br/>
**Below list of keywords are mandatory and cannot be removed**
<table>
    <thead>
        <tr>
            <td width=5%>Keyword</td>
            <td width=20%>Data type</td>
            <td width=35%>Description</td>
            <td width=40%>Possible values</td>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td width=5%>DataFilePath</td>
            <td width=20%>String</td>
            <td width=35%>Path for test data with file name/ test file directory. Can be abosolute or relative. This is not required if your project is fully scriptless as test data is passed from TestCase.json file. Although if you are giving test file directory, also mention TestDataFormat(below mentioned keyword)</td>
            <td width=40%>.\\Project\\TestData\\TestData.xlsx<br/>
            Project\\TestData\\TestData.xlsx<br/>
            C:\\Users\\username\\Desktop\\ProjectName\\Project\\TestData\\TestData.xlsx<br/>
            ./Project/TestData/TestData.xlsx<br/>
            Project/TestData/TestData.xlsx<br/>
            C:/Users/username/Desktop/ProjectName/Project/TestData/TestData.xlsx</td>
        </tr>
        <tr>
            <td width=5%>TestDataFormat</td>
            <td width=20%>String</td>
            <td width=35%>File extension(for excel, only .xlsx file is supported)</td>
            <td width=40%>Excel</td>
        </tr>
        <tr>
            <td width=5%>TestSuiteConfig</td>
            <td width=20%>String</td>
            <td width=35%>Location of test suite config file. Can be abosolute or relative. Only JSON files in provided sample format is accepted.
            </td>
            <td width=40%>C:\\Users\\username\\Desktop\\ProjectName\\BEx\\Framework\\Config\\TestSuite.json<br/>
            C:/Users/username/Desktop/ProjectName/BEx/Framework/Config/TestSuite.json<br/>
            .\\BEx\\Framework\\Config\\TestSuite.json<br/>
            ./BEx/Framework/Config/TestSuite.json<br/>
            BEx\\Framework\\Config\\TestSuite.json<br/>
            BEx/Framework/Config/TestSuite.json</td>
        </tr>
        <tr>
            <td width=5%>TestCasePath</td>
            <td width=20%>String</td>
            <td width=35%>Location of test scripts. Can be abosolute or relative. Only JSON files for scriptless in provided format is accepted.
            </td>
            <td width=40%>.\\Project\\TestScript\\<br/>
            C:/Users/username/Desktop/ProjectName/Project/TestScript<br/>
            C:\\Users\\username\\Desktop\\ProjectName\\Project\\TestScript<br/>
            ./Project/TestScript<br/>
            \\Project\TestScript<br/>
            \\Project\\TestScript</td>
        </tr>
        <tr>
            <td width=5%>SlackNotify</td>
            <td width=20%>Boolean</td>
            <td width=35%>Set true if you want to enable notification of execution results in slack channel. Optional parameter
            </td>
            <td width=40%>true, false</td>
        </tr>
        <tr>
            <td width=5%>ProjectName</td>
            <td width=20%>String</td>
            <td width=35%>Requried if SlackNotify is true. Project name
            </td>
            <td width=40%></td>
        </tr>
        <tr>
            <td width=5%>WebHookUrl</td>
            <td width=20%>String</td>
            <td width=35%>Requried if SlackNotify is true. 
            </td>
            <td width=40%>Your project name</td>
        </tr>
        <tr>
            <td width=5%>Environment</td>
            <td width=20%>String</td>
            <td width=35%>Requried if using Environment variables file. 
            </td>
            <td width=40%>DEV, TEST, UAT</td>
        </tr>
        <tr>
            <td width=5%>RunLocation</td>
            <td width=20%>String</td>
            <td width=35%>Requried if slack notify is true. Default is LOCAL</td>
            <td width=40%>LOCAL/PIPELINE</td>
        </tr>
        <tr>
            <td width=5%>Tag</td>
            <td width=20%>String[]</td>
            <td width=35%>If you want to tag specifi list of persons in channel</td>
            <td width=40%>["@shubhendu.gupta", "@kanika.goyal"]</td>
        </tr>
    </tbody>
</table>

Below are custom examples for `coded` and `scriptless` scripting styles

#### Coded

##### Example 1
```
{
  "DataFilePath": "Project\\TestData\\TestData.xlsx",
   "TestDataFormat": "",
  "TestSuiteConfig": ".\\BEx\\Framework\\Config\\TestSuite.json",
  "TestCasePath": ".\\Project\\TestScript\\"
}
```
##### Example 2
**Below will search for `TestData.xlsx` or `RunManager.xlsx`**
```
{
  "DataFilePath": "Project\\TestData\\",
   "TestDataFormat": "Excel",
  "TestSuiteConfig": ".\\BEx\\Framework\\Config\\TestSuite.json",
  "TestCasePath": ".\\Project\\TestScript\\"
}
```

#### Scriptless
```
{
  "DataFilePath": "Project\\TestData\\",
   "TestDataFormat": "",
  "TestSuiteConfig": ".\\BEx\\Framework\\Config\\TestSuite.json",
  "TestCasePath": ".\\Project\\TestScript\\"
}
```

### Configuring `TestSuite.json`

**File name is strict in nature**
**Below list of keywords are mandatory and cannot be removed**
<table>
    <thead>
        <tr>
            <td width=5%>Keyword</td>
            <td width=20%>Data type</td>
            <td width=35%>Description</td>
            <td width=40%>Possible values</td>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td width=5%>ToExecute</td>
            <td width=20%>String</td>
            <td width=35%>Set "Yes" if you want to execute this test case. "No" if you want to skip this test case</td>
            <td width=40%>Yes, No, yes, no, y, n</td>
        </tr>
        <tr>
            <td width=5%>TestSource</td>
            <td width=20%>String</td>
            <td width=35%>"Coded" if test case is coded test case. "Scriptless" if your test case is scriptless</td>
            <td width=40%>Coded, Scriptless, coded, scriptless</td>
        </tr>
        <tr>
            <td width=5%>Suite</td>
            <td width=20%>String</td>
            <td width=35%>Assign test cases to suites using "Suite" keyword</td>
            <td width=40%>Smoke, Regression, Functional, Sanity, etc.</td>
        </tr>
        <tr>
            <td width=5%>DataKey</td>
            <td width=20%>String</td>
            <td width=35%>Keyword for data driven test cases. You can assign test cases to a data key for driving the test cases from an external souce of data(currently excel). Implementation <a href="#data-driven-test-cases">here</a></td>
            <td width=40%></td>
        </tr>
    </tbody>
</table>


#### Example
```
{
  "TestCaseName": {
    "ToExecute": "Yes",
    "TestSource": "Scriptless",
    "Suite": [ "Regression" ],
    "DataKey": "GetCase"
  },
  "TC001": {
    "ToExecute": "Yes",
    "TestSource": "Coded"
  }
}
```

### Configuring `Environment.json`
If you want to pass sensitive data from a seperate files, you can introduce a new file `Environment.json`.<br/>
This makes the `FrameworkConfig.json` key `Environment` as a **MANDATORY** item.<br/>
Configure it based on your project environments.<br/><br/>
Below is the file format to be used:
```
{
  "DEV": {
    "BaseUrl": "https://dev.beatapps.net/"
  },
  "TEST": {
    "BaseUrl": "https://test.beatapps.net/"
  },
  "UAT": {
    "BaseUrl": "https://uat.beatapps.net/"
  }
}
```
Below are the ways you can call an environment variable:<br/><br/>
To fetch data from current working environment:
```
"EndPoint": "[from-env:BaseUrl]beatflow/oneapp/"
```
To fetch data from specific environment:
```
"EndPoint": "[from-env:UAT:BaseUrl]beatflow/oneapp/"
```

**Only EndPoint and Payload is allowed to use environment variables**<br/>
**Pushing values to environment vairables is not yet implemented. You can use `Store` instead**

## Scripting test cases

### Coded

1. Create test cases in the specified path in `FrameworkConfig.json` file.
2. Add new class file with name `TestCaseName.cs`.
3. Inherit `BEx.Framework.Base.ITestCaseBase` and implement all the methods.
4. Below is the format you should expect:
```
public void AfterSuite(Data data)
{
    throw new NotImplementedException();
}

public void AfterTest(Data data)
{
    throw new NotImplementedException();
}

public void BeforeSuite(Data data)
{
    throw new NotImplementedException();
}

public void BeforeTest(Data data)
{
    throw new NotImplementedException();
}

public void Test(Data data)
{
    throw new NotImplementedException();
}
```
5. Remove all the `throw new NotImplementedException();` autogenerated lines in methods.
6. Start scripting codes in formats.
7. Bussiness components should reside in `root/Project/Components` folder.

### Scriptless

1. Create test cases in the specified path in `FrameworkConfig.json` file.
2. Register test case in `TestSuite.json`
3. Add new JSON file with name `TestCaseName.json`.
```
{
  "TestCaseName": {
    "ToExecute": "Yes",
    "SerialNo": 1,
    "StatusCode": 200,
    "RequestType": "GET",
    "EndPoint": "https://reqres.in/api/users?page=2",
    "Headers": {
      "ContentType": "application/json"
    },
    "ReportResponse": true
  }
}
```
3. Run `.exe` file to run the test case.

## Scriptless keywords

Below are the list of keywords which can be used in any required combinations for test case setup:
<table>
    <thead>
        <tr>
            <td width=5%>Keyword</td>
            <td width=20%>Required/Optional</td>
            <td width=35%>Description</td>
            <td width=20%>Data type</td>
            <td width=20%>Possible values</td>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td width=5%>ToExecute</td>
            <td width=20%>Required</td>
            <td width=35%>Set "Yes" if you want to execute the test step. "No" if you want ot skip this step.</td>
            <td width=20%>String</td>
            <td width=20%>Yes/ No/ yes/ no/ y/ n</td>
        </tr>
        <tr>
            <td width=5%>SerialNo</td>
            <td width=20%>Required</td>
            <td width=35%>Set execution sequence for this step. Cannot be repeatative. Should be consecutive in order(Cannot move from test step 1 to step 3. step 2 is required)</td>
            <td width=20%>Number</td>
            <td width=20%>1</td>
        </tr>
        <tr>
            <td width=5%>StatusCode</td>
            <td width=20%>Required</td>
            <td width=35%>Expected status code for the api call</td>
            <td width=20%>Number</td>
            <td width=20%>200</td>
        </tr>
        <tr>
            <td width=5%>RequestType</td>
            <td width=20%>Required</td>
            <td width=35%>Call type for the api</td>
            <td width=20%>String</td>
            <td width=20%>GET/ POST/ PUT/ DELETE</td>
        </tr>
        <tr>
            <td width=5%>EndPoint</td>
            <td width=20%>Required</td>
            <td width=35%>Api end point</td>
            <td width=20%>String</td>
            <td width=20%>Refer API docs for more details</td>
        </tr>
        <tr>
            <td width=5%>Headers</td>
            <td width=20%>Optional</td>
            <td width=35%>If your call wants headers to be added, add this. If no headers provided, default is <br/>"ContentType": "application/json"</td>
            <td width=20%>Dictionary&ltString, String&gt</td>
            <td width=20%>"Headers": {<br/>
                "ContentType": "application/json"<br/>
            }</td>
        </tr>
        <tr>
            <td width=5%>Parameters</td>
            <td width=20%>Optional</td>
            <td width=35%>If parameters need to be passed from URL, add this to your test case</td>
            <td width=20%>Doctionary&ltString, String&gt</td>
            <td width=20%>{
              "id": "1212"
            }</td>
        </tr>
        <tr>
            <td width=5%>ReportResponse</td>
            <td width=20%>Optional</td>
            <td width=35%>Set true if you want to add whole response to report. Else set false. If not provided, default is false</td>
            <td width=20%>Boolean</td>
            <td width=20%>"ReportResponse": true</td>
        </tr>
        <tr>
            <td width=5%>Store</td>
            <td width=20%>Optional</td>
            <td width=35%>If you want to store a value from request/response payload to framework instance memory,
            use this key-value pair to store data.</td>
            <td width=20%>Dictionary&ltString, String&gt</td>
            <td width=20%><a href="#store">"Store"</a>: {<br/>
                "token": "response:root.access_token"<br/>
            }</td>
        </tr>
        <tr>
            <td width=5%>Expected</td>
            <td width=20%>Optional, Dependant to Validate</td>
            <td width=35%>Actual expected value from response</td>
            <td width=20%>Array of expected values in string(strict)</td>
            <td width=20%>["1212", "Jack"]</td>
        </tr>
        <tr>
            <td width=5%>Validate</td>
            <td width=20%>Optional, Dependant to Expected</td>
            <td width=35%>Actual value validation from response</td>
            <td width=20%>Array of json paths in string(string)</td>
            <td width=20%>["response:root.id", "response:root.firstname"]</td>
        </tr>
        <tr>
            <td width=5%>ShowPayload</td>
            <td width=20%>Optional</td>
            <td width=35%>If you want to view the payload in logs/console, set this key as true</td>
            <td width=20%>Boolean</td>
            <td width=20%>true, false</td>
        </tr>
        <tr>
            <td width=5%>CustomCode</td>
            <td width=20%>Optional</td>
            <td width=35%>If you want to execute any C# code, you can make use of this. For more details, click <a href="#custom-code">here</a></td>
            <td width=20%>String</td>
            <td width=20%>Code inside main()</td>
        </tr>
        <tr>
            <td width=5%>Loop</td>
            <td width=20%>Optional</td>
            <td width=35%>If test step repeatation should be decided over the previous execution, this is useful to this scenario. Click <a href="#loop">here</a> for more details</td>
            <td width=20%>{}</td>
            <td width=20%>"Loop": {<br>
              "Condition": "raw-response==true&&this.StatusCode==200",<br>
              "Count": "10"<br>
            }</td>
        </tr>
        <tr>
            <td width=5%>DownloadFileAs</td>
            <td width=20%>Required</td>
            <td width=35%><b>WARNING:</b>This is an indication that the api call is a going to download the file</td>
            <td width=20%>String</td>
            <td width=20%>FileName.ext</td>
        </tr>
        <tr>
            <td width=5%>EnsureFile</td>
            <td width=20%>Optional</td>
            <td width=35%><b>Useful when user wants to validate the downloadd file in the download path</td>
            <td width=20%>Boolean</td>
            <td width=20%>true/ false</td>
        </tr>
    </tbody>
</table>

## More details

### Custom Code

If your test case requires to execute some complex logic, you can make use of this.

#### Pre code values

```
using System;
using Newtonsoft.Json.Linq;
```

#### Variables

```
String httpResponse;                           // Current response body
String Environment;                             // Current environment
String httpResponseHeaders;              // Current http response headers
Int32 StatusCode;                                // Response status code
String httpRequestPayload;                 // Current request payload
```

#### Shared objects access in custom code
Dynamic data can be accessed using below keyword:
`sharedObjects[KEY]`

For example, if we want to access access_token key, below is the code:
`sharedObjects["access_token"]`

#### Storing values inside CustomCode
If we want to store values inside the custom code to framework memory, you can store the values in a `Dictionary<String, String>()` and return the dictionary values

#### Usage

You need to write the code content inside the `Main()`

For example, for parsing and printing the response JSON:
```
JObject obj = JObject.Parse(httpResponse);
Console.WriteLine(httpResponse);
```

### Loop

#### Syntax format

```
"Loop": {
      "Condition": "raw-response==true&&this.StatusCode==200",
      "Count": "10"
}
```

`Condition` : This is a raw condition which is needed to <b>Break out from the loop</b>
In the condition shown above, to break the loop, `raw-response==true&&this.StatusCode==200` has to be satisfied

`Count` : This is the maximum number of execution, the step should repeat if the condition fails. Default is 10.

### Data Driven Test Cases

If an API need to be tested over multiple sets of data/ from an external souce of data, you can make use of this.
In order to data drive a test case, follow below steps:

1. Create an xlsx workbook in `root/Project/Datatable` folder
2. Add the data key to `TestSuite.json` file as mentioned below:
```
  "ToExecute": "Yes",
      "TestSource": "Scriptless",
      "Suite": [ "Regression" ],
      "DataKey": "GetCase"
```

where `GetCase` is the data key.

3. Add excel sheet with same name as mentioned as DataKey. In the above case, excel sheet should be of name as `GetCase`.
4. Add your keyword data set in this sheet save the sheet
5. Now if you dont add those keys to `TestCase.json` file also, it will not give any error.
6. Run your test cases.

### Scope of data validations

Data validations from an API response can be categorized into 2 parts:
  - Simple: Where validations are straight and no complex code logic is required
  - Complex: Where validations requires code logics like loops, iterations are required

When `Simple` validations are considered, we can use `Validate` and `Expected` keys to validate data.

When `Complex` validations are considered, we can use `CustomCodes` with `Expected` keys.

### Keywords extensions

#### `[Fetch:<JSONPath_TO_VARIABLE>]`
If you want to fetch any stored value from the framework memory and want to append it to any other keyword, use 
`[Fetch:<JSONPath_TO_VARIABLE>]` to fetch and append.

For example, if you want to append stored value to `EndPoint` keyword:
```
    "EndPoint": "https://reqres.in/api/users?user=[Fetch:id]"
```
**Necessary condition to use this is, you should store values before using this**

#### `upload-file:<PATH/TO/FILE/RELATIVE/TO/DataFilePath>`
If you have multipart form data to upload file, use this extension to upload the file.

Below is an example to this:

```
...
"Payload": {
  "files": "upload-file:Uploads\\SampleFile.pdf"
}
...
```

If you wants to upload multiple files, you can call this keyword, but the values to `upload-file:` should be array type.<br/>
Below is an example:

```
...
"Payload": {
  "files": "upload-file:[Uploads\\SampleFile.pdf,Uploads\\SampleFile.pdf]"
}
...
```

#### `from-file:<PATH/TO/FILE/RELATIVE/TO/DataFilePath>`
If you want to pass any data from external file content, use this extension to the Keyword.

Below is an example to this:
```
...
"Payload": {
  "user_details": "[from-file:Payloads\\Payload.txt]"
}
...
```

#### `from-code:<PATH/TO/CODE/FILE/RELATIVE/TO/DataFilePath.extension>`
If you want to execute some custom codes within your execution, you can use this keyword

Below are some examples to use this:

```
...
"Payload": {
  "user_details": "[from-code:Scripts\\Code.txt]"
}
...
```
```
...
"Payload": {
  "from-code": "Scripts\\Code.cs"
}
...
```
```
...
"Parameters": {
  "from-code": "Scripts\\Payload.cs"
}
...
```
For coding related documents, refer [here](#custom-code)

#### `to-file:<REQUEST/RESPONSE>:<JSONPath_TO_VARIABLE>`
If you want to store data which is returned from `<REQUEST/RESPONSE>` after reading `<JSON_TO_VARIABLE>`,
you can use this extension to the keyword.
The stored file will be named as: `TestStepName_json.path.txt`

Below is an example to this:
```
...
"Store": {
  "to-file": "response:root.id"
}
...
```
Above will fetch `root.id` from `response` and store to file with name `TestStepName_root.id.txt`

#### `raw-response:root`
If the response payload is not a JSON type, you can use this extension with `Store`. It can also be used with
`to-file`.

Below is an example to this:
```
...
"Store": {
  "to-file": "raw-response:root"
}
...
```

### Custom inbuild functions

#### RandomName(length)

If you want to generate random strings of `length` number of characters, you can use this.


Usage:

Below generates random string of 10 characters
```
"Payload": {
  "name": "RandomName(10)"
}
```

#### GetDate(format)

If you want to generate datetime in mentioned `format` you can use this.

Usage:

Below generates date of `dddd, dd MMMM yyyy` format
```
"Payload": {
  "name": "GetDate(\"dddd, dd MMMM yyyy\")"
}
```
Refer [here](#date-time-formats) for list of supported date time formats

### Extended Features

#### Data drive a test step
If you want to data drive a test step, you can follow the below steps:

1. Create an xlsx workbook in `root/Project/Datatable` folder
2. Add the data key to `TestSuite.json` file as mentioned below:
```
  "ToExecute": "Yes",
      "TestSource": "Scriptless",
      "Suite": [ "Regression" ],
      "DataKey": "GetCase"
```

where `GetCase` is the data key.

3. Add excel sheet with same name as mentioned as DataKey. In the above case, excel sheet should be of name as `GetCase`.
4. Add your keyword data set in this sheet save the sheet
5. Now if you dont add those keys to `TestCase.json` file also, it will not give any error.
6. Run your test cases.

#### API Segregation

If you want to pass the test steps from a seperate files, you can use this feature.

**NOTE** API segregation cannot be used for only 1 API in a test case. That means if you want to segregate 1 API,
in a test case, you should segregate all other APIs also in the same test case.

Belo are the steps to implement this feature:
1. Add a key `ApiLibraryPath` in FrameworkConfig.json:
```
{
  ...
  "ApiLibraryPath": "ApiLibrary",
  ...
}
```
2. Copy the test steps to a file with the same name as the test step name in the `ApiLibraryPath` directory. That means, if your test step name is `ListAllValues`, file name in `ApiLibraryPath` directory should be `ListAllValues.json`
3. Add all the test steps in array form in your test case file.

#### `Before` feature in testsuites

If you want to pass a list of pre-requiste test steps which will be ran before test case execution has started,
you can use `Before` in `TestSuite.json`. Below is the rule:

```
{
  "Before": "BeforeSteps",
  "GetAllCases": {
    "ToExecute": "Yes",
    "TestSource": "Scriptless",
    "Suite": [ "Regression" ],
    "DataKey": "GetCase"
  },
  "SuccessfulTransaction": {
    "ToExecute": "Yes",
    "TestSource": "Scriptless",
    "Suite": [ "Regression" ],
    "DataKey": "PostCase"
  }
}
```
where `BeforeSteps` is a test case file, residing in TestScipts folder with same format as test cases.

##### Example

Let us say your test case file looks like this:
```
{
  "TestStep1": {
    "RequestType": "GET",
    "SerialNo": 1,
    "ToExecute": "Yes",
    "EndPoint": "[from-env:BaseUrl]api/[from-file:Endpoint\\GET_Test_With_Parameters_Store_Test.txt]",
    "Parameters": {
      "from-file": "Parameters\\GET_GET_Test_With_Parameters_Store_Test.json"
    },
    "StatusCode": 200,
    "Loop": {
      "Condition": "this.StatusCode==200",
      "Count": "5"
    },
    "ReportResponse": true,
    "Validate": [ "response:root.page" ],
    "Expected": [ "2" ],
    "Store": [
      {
        "page": "response:root.page"
      }
    ]
  },
  "TestStep2": {
    "RequestType": "GET",
    "SerialNo": 1,
    "ToExecute": "Yes",
    "EndPoint": "[from-env:BaseUrl]api/[from-file:Endpoint\\GET_Test_With_Parameters_Store_Test.txt]",
    "Parameters": {
      "from-file": "Parameters\\GET_GET_Test_With_Parameters_Store_Test.json"
    },
    "StatusCode": 200,
    "Loop": {
      "Condition": "this.StatusCode==200",
      "Count": "5"
    },
    "ReportResponse": true,
    "Validate": [ "response:root.page" ],
    "Expected": [ "2" ],
    "Store": [
      {
        "page": "response:root.page"
      }
    ]
  }
}
```

Create files in `ApiLibraryPath` as names: `TestStep1` and `TestStep2` and copy the test step contents to the respective files.
And atleast, change the TestCase file content from above to:
```
[
  "TestStep1",
  "TestStep2"
]
```

## Commandline options

### --tests=<Comma,Seperated,Tests>
If you want to run specific test cases, you can provide the test case name as command line arguments to `BEx.exe`.<br/>
The format for providing argument is provided below:

`BEx --tests=TC001`

If more than 1 test cases are to be provided, you can append them as comma seperated values. Below is the format:

`BEx --tests=TC001,TC002,...`

### --toreport=false
During the script development, we dont want to generate reports as it becomes a heavy array of auto generated files. In that cases, where you don't want to generate reports, you can use this argument like below:

`BEx --toreport=false`

**NOTE: You can use the arguments with any combinations**

### --suite=&lt;SuiteName&gt;
If you have segregated your tests with `Suite` in `TestSuite.json`, you can run your tests which are tagged to some suite name.

Use below command to run your tests:

`bex --suite=Smoke`

### Other Options

#### --version
To list the current version of BEx, you can use this option

`BEx --version`

### --update@version
If you want to download a specific version of the framework, they can use this option which will download the mentioned version in the `libs` folder.

Usage:

`BEx --update@x.x.x`

If you want to download at specific path, you can add one more option `--path=DownloadPath` like below:

`BEx --update@x.x.x --path=DownloadPath`

### --summary
You can generate a framework summary report containing number of test case and number of apis automated using this command.

`BEx --summary`

## Slack Notifications

Latest release is capable to send execution reports in slack channel. The default notifications are sent to a
channel named `#automation`. This accepts three more keys in `FrameworkConfig.json`: `SlackNotify`, `ProjectName`, `WebHookUrl`, `Channel`, `Tag`, `RunLocation`.
This feature will look for other keys only if you provide `SlackNotify` as `true`.

After successful implementation, you will receive message in below format:

<img src="https://github.com/MAKnowledgeServices/BEx-Framework/blob/main/images/slack_notification.PNG?raw=true" alt="logo"><br/>

For creating new webhooks and modifying refer to wiki docs.

## AWS CodePipeline Integration

AWS Codepipeline integration is required for CI/CD pipeline integrations. Below are the list of files while are to be placed in project root:

1. `appspec.yml`
2. `BeforeInstall.ps1`
3. `buildspec.yml`
4. `run.bat`

### Project specific customisations for AWS files

#### `appspec.yml`

At `line number:5` change according to project name and save:

`destination: C:\inetpub\wwwroot\PROJECT_NAME`

#### `BeforeInstall.ps1`

At `line numner:15` change according to project name and save:

`Remove-Item "C:\inetpub\wwwroot\PROJECT_NAME\*" -Recurse`

#### `buildspec.yml`

**No change required**

#### `run.bat`

At `line numner:1` change according to project name and save:

`cd C:\inetpub\wwwroot\PROJECT_NAME\libs`

For `coded` scripts, change `line number:2` to your project `.exe` file.

## Docker Integration

**Please ensure you have docker instaled in your local system**

Below are the list of files while are to be placed in project root:

1. Dockerfile
2. run.bat

#### Project specific customisations for Dockerfile

At `line number:3, 4, 5` modify based on following and save:

```
ADD Framework/ inetpub/wwwroot/PROJECT_NAME/Framework
ADD libs/ inetpub/wwwroot/PROJECT_NAME/libs
ADD Project/ inetpub/wwwroot/PROJECT_NAME/Project
```

### Date Time Formats
<table>
  <thead>
    <tr>
      <td>Format</td>
      <td>Result</td>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td>DateTime.Now.ToString("MM/dd/yyyy")</td>
      <td>05/29/2015</td>
    </tr>
    <tr>
      <td>DateTime.Now.ToString("dddd, dd MMMM yyyy")</td>
      <td>Friday, 29 May 2015</td>
    </tr>
    <tr>
      <td>DateTime.Now.ToString("dddd, dd MMMM yyyy")</td>
      <td>Friday, 29 May 2015 05:50</td>
    </tr>
    <tr>
      <td>DateTime.Now.ToString("dddd, dd MMMM yyyy")</td>
      <td>Friday, 29 May 2015 05:50 AM</td>
    </tr>
    <tr>
      <td>DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss")</td>
      <td>Friday, 29 May 2015 05:50:06</td>
    </tr>
    <tr>
      <td>DateTime.Now.ToString("MM/dd/yyyy HH:mm")</td>
      <td>05/29/2015 05:50</td>
    </tr>
    <tr>
      <td>DateTime.Now.ToString("MM/dd/yyyy hh:mm tt")</td>
      <td>05/29/2015 05:50 AM</td>
    </tr>
    <tr>
      <td>DateTime.Now.ToString("MM/dd/yyyy H:mm")</td>
      <td>05/29/2015 5:50</td>
    </tr>
    <tr>
      <td>DateTime.Now.ToString("MM/dd/yyyy h:mm tt")</td>
      <td>05/29/2015 5:50 AM</td>
    </tr>
    <tr>
      <td>DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")</td>
      <td>05/29/2015 05:50:06</td>
    </tr>
    <tr>
      <td>DateTime.Now.ToString("MMMM dd")</td>
      <td>May 29</td>
    </tr>
    <tr>
      <td>DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK")</td>
      <td>2015-05-16T05:50:06.7199222-04:00</td>
    </tr>
    <tr>
      <td>DateTime.Now.ToString("ddd, dd MMM yyy HH':'mm':'ss 'GMT'")</td>
      <td>Fri, 16 May 2015 05:50:06 GMT</td>
    </tr>
    <tr>
      <td>DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH:'mm':'ss")</td>
      <td>2015-05-16T05:50:06</td>
    </tr>
    <tr>
      <td>DateTime.Now.ToString("HH:mm")</td>
      <td>05:50</td>
    </tr>
    <tr>
      <td>DateTime.Now.ToString("hh:mm tt")</td>
      <td>05:50 AM</td>
    </tr>
    <tr>
      <td>DateTime.Now.ToString("H:mm")</td>
      <td>5:50</td>
    </tr>
    <tr>
      <td>DateTime.Now.ToString("h:mm tt")</td>
      <td>5:50 AM</td>
    </tr>
    <tr>
      <td>DateTime.Now.ToString("HH:mm:ss")</td>
      <td>05:50:06</td>
    </tr>
    <tr>
      <td>DateTime.Now.ToString("yyyy MMMM")</td>
      <td>2015 May</td>
    </tr>
  </tbody>
</table>

`d` -> Represents the day of the month as a number from 1 through 31.

`dd` -> Represents the day of the month as a number from 01 through 31.

`ddd`-> Represents the abbreviated name of the day (Mon, Tues, Wed, etc).

`dddd`-> Represents the full name of the day (Monday, Tuesday, etc).

`h`-> 12-hour clock hour (e.g. 4).

`hh`-> 12-hour clock, with a leading 0 (e.g. 06)

`H`-> 24-hour clock hour (e.g. 15)

`HH`-> 24-hour clock hour, with a leading 0 (e.g. 22)

`m`-> Minutes

`mm`-> Minutes with a leading zero

`M`-> Month number(eg.3)

`MM`-> Month number with leading zero(eg.04)

`MMM`-> Abbreviated Month Name (e.g. Dec)

`MMMM`-> Full month name (e.g. December)

`s`-> Seconds

`ss`-> Seconds with leading zero

`t`-> Abbreviated AM / PM (e.g. A or P)

`tt`-> AM / PM (e.g. AM or PM

`y`-> Year, no leading zero (e.g. 2015 would be 15)

`yy`-> Year, leading zero (e.g. 2015 would be 015)

`yyy`-> Year, (e.g. 2015)

`yyyy`-> Year, (e.g. 2015)

`K`-> Represents the time zone information of a date and time value (e.g. +05:00)

`z`-> With DateTime values represents the signed offset of the local operating system's time zone from

      Coordinated Universal Time (UTC), measured in hours. (e.g. +6)

`zz`-> As z, but with leading zero (e.g. +06)

`zzz`-> With DateTime values represents the signed offset of the local operating system's time zone from UTC, measured in hours and minutes. (e.g. +06:00)

`f`-> Represents the most significant digit of the seconds' fraction; that is, it represents the tenths of a second in a date and time value.

`ff`-> Represents the two most significant digits of the seconds' fraction in date and time

`fff`-> Represents the three most significant digits of the seconds' fraction; that is, it represents the milliseconds in a date and time value.

`ffff`-> Represents the four most significant digits of the seconds' fraction; that is, it represents the ten-thousandths of a second in a date and time value. While it is possible to display the ten-thousandths of a second component of a time value, that value may not be meaningful.

`fffff`-> Represents the five most significant digits of the seconds' fraction; that is, it represents the hundred-thousandths of a second in a date and time value.

`ffffff`-> Represents the six most significant digits of the seconds' fraction; that is, it represents the millionths of a second in a date and time value.

`fffffff`-> Represents the seven most significant digits of the second's fraction; that is, it represents the ten-millionths of a second in a date and time value.

### Credits
- Shubhendu Shekhar Gupta