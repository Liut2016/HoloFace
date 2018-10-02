using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
using UnityEngine.UI;

public class GetPicture : MonoBehaviour {

    
    private Image image;


    // Use this for initialization
    void GetPhoto()
    {
        string filepath = Path.Combine(Application.persistentDataPath, "cropped.png");
        Debug.Log("filepath : " + filepath);
        //   string filepath = "C:/Data/Users/super/AppData/Local/Packages/HoloFace2_pzq3xp76mxafg/LocalState\\cropped.png";
     //   Image image = Instantiate(imagePrefab);
        image = GetComponent<Image>();




        double startTime = (double)Time.time;
        //创建文件流
        FileStream fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
        fileStream.Seek(0, SeekOrigin.Begin);
        //创建文件长度的缓冲区
        byte[] bytes = new byte[fileStream.Length];
        //读取文件
        fileStream.Read(bytes, 0, (int)fileStream.Length);
        //释放文件读取liu
        //   fileStream.Close();
        fileStream.Dispose();
        fileStream = null;

        //创建Texture
        int width = 300;
        int height = 372;
        Texture2D texture2D = new Texture2D(width, height);
        texture2D.LoadImage(bytes);

        Sprite sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height),
            new Vector2(0.5f, 0.5f));
        image.sprite = sprite;
        double time = (double)Time.time - startTime;
        Debug.Log("IO加载用时：" + time);



        //IO方法加载速度快
      //  LoadByIO(filepath);
    }


    /*
    void LoadByIO(string filepath)
    {
        double startTime = (double)Time.time;
        //创建文件流
        FileStream fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
        fileStream.Seek(0, SeekOrigin.Begin);
        //创建文件长度的缓冲区
        byte[] bytes = new byte[fileStream.Length];
        //读取文件
        fileStream.Read(bytes, 0, (int)fileStream.Length);
        //释放文件读取liu
     //   fileStream.Close();
        fileStream.Dispose();
        fileStream = null;

        //创建Texture
        int width = 300;
        int height = 372;
        Texture2D texture2D = new Texture2D(width, height);
        texture2D.LoadImage(bytes);

        Sprite sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height),
            new Vector2(0.5f, 0.5f));
        image.sprite = sprite;
        double time = (double)Time.time - startTime;
        Debug.Log("IO加载用时：" + time);
    }*/
}
