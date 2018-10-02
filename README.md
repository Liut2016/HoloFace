# Finally done on June 9, 2018    

This is a face recognition application based on augmented reality technology. The operating platform is Microsoft HoloLens.  

## Development environment  

Windows 10  

Unity 2017.3.1f1  


## function  

+ Face detection & face recognition  

The face API for Microsoft Cognitive Services is used in the project, reference https://azure.microsoft.com/en-us/services/cognitive-services/face/  

The face recognition function is implemented in MsFaceIdentify.cs. You only need to change the code in the PostToFaceAPI() method to add your own face recognition algorithm.  

+ Environmental understanding  

Imaging API based on Microsoft Cognitive Services, References https://azure.microsoft.com/en-us/services/cognitive-services/computer-vision/  

Environmental understanding is implemented in ComputerVision.cs  

+ Speech Recognition  

Based on the API provided by BosonNLP, reference material https://bosonnlp.com/  

A simple semantic understanding algorithm is implemented in the PostToBosonNLPAPI() method, in which a custom grammar rule can be implemented to achieve a specific semantic understanding function.  

+ BillBoard  

A function similar to the menu menu is implemented, and the interface follows the user's line of sight, reflecting the characteristics of augmented reality.  


## System  

![Image text](https://github.com/Liut2016/HoloFace/blob/master/PictureInDoc/System.png)  


## File  

+ GazeGestureManager.cs //Menu  

+ MsFaceIdentify.cs     //Face recognition algorithm  

+ SetCanvas.cs	      //Face recognition result display  

+ ComputerVision.cs     //Environmental understanding algorithm  

+ SetSurroundings.cs    //Environmental understanding result display  

+ DictonManager.cs      //Dictation function, speech recognition  

+ FaceRememberLogic.cs //Enter strange faces into the cloud database	  

+ SetRememberCanvas.cs  //Return to the face entry process  

+ GetPicture.cs		  //Get photos in HoloLens  

+ SpeechManager.cs      //Key words  


## Result picture  

### Menu  

![Image text](https://github.com/Liut2016/HoloFace/blob/master/PictureInDoc/0.jpg)  

### Environmental understanding  

![Image text](https://github.com/Liut2016/HoloFace/blob/master/PictureInDoc/1.jpg)  


## Operation instructions  

At present, the environment has undergone major changes, so direct operation may report an error. It is recommended to run according to your own environment.  

Since the trial version of the Microsoft Cognitive Services API is used in this project, it has expired, so some functions are no longer available. It is recommended to replace it with your own API KEY or implement your own algorithm.  


## Attention!  

Please note the WWW method on Unity 2017.3.1f1, which may not be available on this version due to design flaws. There are related posts in this Unity forum.  



# 最终完成于2018年6月9日  


这是一款基于增强现实技术的人脸识别应用，运行平台为微软HoloLens。  


## 开发环境：  

Windows 10  

Unity 2017.3.1f1  


## 实现功能：  

+ 人脸检测、人脸识别  

项目中使用了微软认知服务的人脸API，参考资料 https://azure.microsoft.com/zh-cn/services/cognitive-services/face/  

人脸识别功能在MsFaceIdentify.cs中实现，只需更改PostToFaceAPI()方法中的代码即可添加自己的人脸识别算法  

+ 环境理解  

基于微软认知服务的影像API，参考资料 https://azure.microsoft.com/zh-cn/services/cognitive-services/computer-vision/  

环境理解功能在ComputerVision.cs中实现  

+ 语音识别  

基于BosonNLP提供的API，参考资料https://bosonnlp.com/  

在PostToBosonNLPAPI()方法中实现了简单的语义理解算法，在该方法中自定义语法规则即可实现特定的语义理解功能  

+ BillBoard(公告牌)  

实现了类似目录菜单的功能，界面跟随用户视线移动，体现出增强现实的特点。  


## 系统架构  

![Image text](https://github.com/Liut2016/HoloFace/blob/master/PictureInDoc/System.png)  


## 文件说明  

+ GazeGestureManager.cs //目录菜单功能  

+ MsFaceIdentify.cs     //人脸识别算法实现  

+ SetCanvas.cs	      //人脸识别结果展示  

+ ComputerVision.cs     //环境感知功能实现  

+ SetSurroundings.cs    //环境感知结果展示  

+ DictonManager.cs      //听写功能，语音识别  

+ FaceRememberLogic.cs //将陌生人脸录入云数据库	  

+ SetRememberCanvas.cs  //返回人脸录入过程  

+ GetPicture.cs		  //得到HoloLens拍照的照片  

+ SpeechManager.cs      //语音识别（关键词）  


## 运行图片  

### 目录菜单  

![Image text](https://github.com/Liut2016/HoloFace/blob/master/PictureInDoc/0.jpg)  

### 环境感知  

![Image text](https://github.com/Liut2016/HoloFace/blob/master/PictureInDoc/1.jpg)  


## 运行说明  

目前环境已经发生了较大变化，因此直接运行可能会报错，建议根据自己的环境进行调整后运行  

由于该项目中使用了微软认知服务API的试用版，目前已经过期，因此部分功能已经无法使用，建议更换为自己的API KEY或者实现自己的算法  


## 特别提醒  

请注意Unity 2017.3.1f1版本上WWW方法，该方法由于设计缺陷在该版本上可能无法使用，对此Unity论坛中有相关的帖子  
