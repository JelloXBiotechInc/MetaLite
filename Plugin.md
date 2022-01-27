Parameter Spec(C# Viewer): 
===
```Variable with * is optional input.```

#### ***string** FunctionName: 
> Viewer will auto place with plugin file name.

#### **string** PostUri: 
> The Server function path.

#### **bool** IsSendImage:
> Is send image to server as function input.

#### **bool** IsSendMask:
> Is send current annotation to server as function input.

#### **List<Column>** Content:
> The plugin function parameter of UI window input that will auto create by these object.
- **class** Column
  + **string** parameterType:    
    > Supported type:
    >>  **ComboBox**
        **RangeInt32**
        **SliderInt32**

  - **string** title:
    > The title shows in UI window.

  - **string** parameterName:
	> The parameter name.
  - **bool** auto:
    > If the parameter can be execute as auto adjust by function feature. If click auto the adjust block will be hide.
  - ***List<Data>** datas:
    > The datas of multiple or group datatype, dependence on:
    >> **ComboBox**
    * **class** Data:
      - **string** varName:
         > The ComboBoxItem variable name.
      - **string** value:
         > The default value of each ComboBoxItem.
  - ***List<string>** items:
       > The option of ComboBox dropdown, dependence on. Put empty string if the parameter can be empty.
       >> **ComboBox**
  - ***class** Range:
       > The digital value setting of number variable, dependence on:
       >> **RangeInt32**
       >> **SliderInt32**
    - **double** from
        > The default value of number type parameter
    - ***double** to
        > The default value of Range type parameter 
        dependence on **RangeInt32**
    - ***double** min
        > If not null, The minimun number of this parameter. 
        dependence on **RangeInt32**, **SliderInt32**
    - ***double** max
        > If not null, The minimun number of this parameter. 
        dependence on **RangeInt32**, **SliderInt32**
        
:::spoiler Example:
``` Json
{
  "FunctionName": "ParallelStatistic",
  "PostUri": "/jellox/ki67_percentage_test",
  "IsSendImage": true,
  "IsSendMask": true,
  "Content": [
    {
      "parameterType": "ComboBox",
      "title": "Channel Select",
      "parameterName": "ChannelSelect",
      "datas": [
        {
          "varName": "red_channel",
          "value": "Nuclear"
        },
        {
          "varName": "green_channel",
          "value": "Antibody"
        },
        {
          "varName": "blue_channel",
          "value": "Membrane"
        }
      ],
      "items": [ "", "Nuclear", "Antibody", "Membrane" ]
    },
    {
      "parameterType": "ComboBox",
      "title": "Apparent magnification",
      "parameterName": "ApparentMagnification",
      "datas": [
        {
          "varName": "ApparentMagnification",
          "value": "20x"
        }
      ],
      "items": [ "20x", "40x" ]
    },
    {
      "parameterType": "RangeInt32",
      "title": "Cell size range",
      "parameterName": "CellRange",
      "auto": true,
      "Range": {
        "Min": "0",
        "Max": "1000",
        "From": "10",
        "To": "40"
      }
    },
    {
      "parameterType": "SliderInt32",
      "title": "Threshold",
      "parameterName": "Threshold",
      "auto": true,
      "Range": {
        "Min": "0",
        "Max": "255",
        "From": "127"
      }

    }
  ]
}
```
:::

Return Json Spec(Python Server):
===
Use dictionary to return data. 
The return variable and image can be more then one. Just put them in one dictionary.
## Variable spec.
#### **List<Data>**
> If the data type is **Number** it will be auto plot at chart panel, and data grid. 
> If the data type is string it will be only shows in data grid.
- **class** Data:
    - **string** varName:
        > The return variable name
    - **object** value
        > The return value can be string, double, bool
  
#### **List<Image>**
The value is use ```.tolist()``` function to convert form numpy to list array, then jsonfy to be send.
- **class** Image:
    - **string** 
      > The title that will be shows on annotation layer panel.
    - **list** value 
      > Image data.
    - ***string** colorString
      > The annotation color that will be shows in MetaLite viewer.
      
:::spoiler Example:
``` python
result_list = {
  "Datas": [
    {"varName": "Percentage", "value": Percentage}, 
    {"varName": "Expression", "value": Expression}, 
    {"varName": "test", "value": random.randint(50,100)}
  ],
  "Images": [ 
    {"imageName": "CellArea", "value": image.tolist(), "colorString": "#FF0000"},
    {"imageName": "CellArea", "value": image.tolist(), "colorString": "#00FF00"},
    {"imageName": "CellArea", "value": image.tolist(), "colorString": "#0000ff"}
  ]
}
```
:::