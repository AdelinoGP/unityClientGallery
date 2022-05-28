using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class galleryHandler : MonoBehaviour{

    [SerializeField]private Sprite transparentSprite;
    [SerializeField]private Image imageSingleton;
    [SerializeField]private Image canvas;
    [SerializeField]private Dropdown genreDropdown;
    private List<Image> imageObjects;

    [Serializable]public class MyImage{
        public int id;
        public string name;
        public string genre;
        [NonSerialized] public string checkSum;
        [NonSerialized] public Sprite file;
    }

    [Serializable]
    public struct ImageListWrapper { public MyImage[] images; }
    private List<MyImage> imageList;

    IEnumerator GetImageList()
    {
        UnityWebRequest www = UnityWebRequest.Get("http://localhost:8000/imageGallery/getList");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);

        else
        {
            var list = JsonUtility.FromJson<ImageListWrapper>("{\"images\":" + www.downloadHandler.text + "}");

            imageList.Clear();
            imageList.AddRange(list.images);

            List<string> genreOptions = new List<string> { "All" };
            genreOptions.AddRange(imageList.ConvertAll(x => x.genre));
            genreOptions = genreOptions.Distinct().ToList();
            genreDropdown.ClearOptions();
            genreDropdown.AddOptions(genreOptions);

            canvas.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ((float)Math.Ceiling((Double)imageList.Count / 2) * 350) + 1000f);

            for (int i = imageObjects.Count; i < imageList.Count; i++)
                imageObjects.Add(Instantiate(imageSingleton, new Vector3( i % 2 == 0 ? 560 : 160 , ((float)Math.Ceiling((Double)(i-3)/2) * -190.0f)+80f, 0), Quaternion.identity,canvas.transform));

            yield return StartCoroutine(GetImages(imageList));
        }
    }

    IEnumerator GetImages(List<MyImage> imageList)
    {
        for(int i = 0; i<imageList.Count;i++)
        {
            if(imageObjects[i].sprite.name != imageList[i].name)
            {
                UnityWebRequest www = UnityWebRequestTexture.GetTexture("http://localhost:8000/imageGallery/" + imageList[i].name);
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                    Debug.Log(www.error);
                else
                {
                    Texture2D myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture as Texture2D;
                    imageList[i].file = Sprite.Create(myTexture, new Rect(0.0f, 0.0f, myTexture.width, myTexture.height), new Vector2(0.5f, 0.5f), 100.0f);
                    imageObjects[i].sprite = imageList[i].file;
                    imageObjects[i].sprite.name = imageList[i].name;
                }
            }
        }
    }
    

    public void updateImageList()
    {
        StartCoroutine(GetImageList());
    }

    public void filterImages()
    {
        int j = 0;
        string genreChosen = genreDropdown.captionText.text;
        for (int i = 0; i < imageList.Count; i++)
        {
            Debug.Log(imageList[i].genre + genreChosen);
            if (imageList[i].genre == genreChosen || genreChosen == "All")
            {
                imageObjects[i].transform.position = new Vector3(j % 2 == 0 ? 560 : 160, ((float)Math.Ceiling((Double)(j - 3) / 2) * -190.0f), 0);
                imageObjects[i].enabled = true;
                j++;
            }
            else
            {
                imageObjects[i].enabled = false;
            }
        }
    }


    void Start()
    {
        imageObjects = new List<Image>();
        imageList = new List<MyImage>();
        StartCoroutine(GetImageList());
    }


    void Update()
    {
        
    }
}
