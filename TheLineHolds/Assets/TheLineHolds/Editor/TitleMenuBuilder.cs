using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace TheLineHolds.Editor
{
    public static class TitleMenuBuilder
    {
        const string Art="Assets/TheLineHolds/UI/FieldManual/";
        static Color Ink=>DioramaSurface.Hex("26322F");
        static Color Gold=>DioramaSurface.Hex("D4AE6B");
        static Color Ivory=>DioramaSurface.Hex("F2EADD");
        static Color Rule=>DioramaSurface.Hex("63706B");

        [MenuItem("The Line Holds/UI/Build Main Menu")]
        public static void Build()
        {
            if(EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().isDirty)
                throw new InvalidOperationException("Save the active scene in Edit Mode before rebuilding the main menu.");
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var camera=new GameObject("Title camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>();
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Ink;camera.cullingMask=0;
            var canvas=new GameObject("Title screen",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),
                typeof(GraphicRaycaster),typeof(TitleMenu));
            canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var menu=canvas.GetComponent<TitleMenu>();
            var cloth=Rect(canvas.transform,"Bookcloth",0,0,1,1);
            cloth.pivot=new Vector2(.5f,.5f);cloth.anchorMin=Vector2.zero;cloth.anchorMax=Vector2.one;cloth.offsetMin=cloth.offsetMax=Vector2.zero;
            var cover=cloth.gameObject.AddComponent<RawImage>();cover.texture=Texture("Bookcloth");
            cover.raycastTarget=false;
            var tiled=cloth.gameObject.AddComponent<AspectRatioFitter>();
            // The cloth is a full-bleed landscape print. Cover every window aspect.
            tiled.aspectRatio=1.6f;tiled.aspectMode=AspectRatioFitter.AspectMode.EnvelopeParent;
            var content=Rect(canvas.transform,"Battlefield selection",0,0,1280,744);
            content.anchorMin=content.anchorMax=content.pivot=new Vector2(.5f,.5f);
            content.anchoredPosition=Vector2.zero;menu.content=content;
            var header=Rect(content,"The Line Holds",0,6,800,115).gameObject.AddComponent<RawImage>();
            header.texture=Texture("Wordmark");header.color=Ivory;header.raycastTarget=false;
            var stamp=Rect(content,"Service insignia",1148,0,132,132).gameObject.AddComponent<RawImage>();
            stamp.texture=Texture("Insignia");stamp.raycastTarget=false;
            Line(content,0,156,1280,1,Rule);
            Label(content,"Choose your battlefield",0,180,700,55,32,Ivory,FontStyles.Bold);
            Label(content,"Hold the road through ten attacks.",710,186,570,44,21,Ivory,
                alignment:TextAlignmentOptions.MidlineRight);
            menu.countryside=Card(content,0,"Countryside","NORMANDY","Defend the farm road.","Countryside");
            menu.winterForest=Card(content,432,"Winter Forest","SIGNAL POST","Hold the snowbound switchback.","WinterForest");
            menu.saintAubin=Card(content,864,"Saint-Aubin","MARKET SQUARE","Hold the town hall.","SaintAubin");
            Line(content,0,695,1280,1,Rule);
            menu.quit=Action(content,"Quit game",1150,707,130,40,Ink);
            Label(menu.quit.transform,"Quit game",0,0,130,40,19,Ivory,alignment:TextAlignmentOptions.Center);
            Link(menu.countryside,menu.saintAubin,menu.winterForest,menu.quit);
            Link(menu.winterForest,menu.countryside,menu.saintAubin,menu.quit);
            Link(menu.saintAubin,menu.winterForest,menu.countryside,menu.quit);
            Link(menu.quit,menu.countryside,menu.saintAubin,menu.saintAubin);
            new GameObject("Menu input",typeof(EventSystem),typeof(InputSystemUIInputModule));
            EditorSceneManager.SaveScene(scene,PrototypeBuilder.MenuScene);
            EditorBuildSettings.scenes=PrototypeBuilder.BuildScenes.Select(p=>new EditorBuildSettingsScene(p,true)).ToArray();
            AssetDatabase.SaveAssets();
        }

        static Button Card(Transform parent,float x,string name,string location,string brief,string preview)
        {
            const float w=416, photoHeight=285;
            var button=Action(parent,name,x,252,w,416,DioramaSurface.Hex("D8D0BF"));
            var card=button.transform;
            var photo=Rect(card,name+" preview",8,8,w-16,photoHeight).gameObject.AddComponent<RawImage>();
            photo.texture=Texture(preview);photo.raycastTarget=false;
            float aspect=(w-16)/photoHeight*photo.texture.height/photo.texture.width;
            photo.uvRect=aspect<1?new Rect((1-aspect)/2,0,aspect,1):new Rect(0,(1-1/aspect)/2,1,1/aspect);
            Label(card,location,22,301,w-48,23,15,Ink,FontStyles.Bold,spacing:5);
            Label(card,name,20,324,w-110,50,39,Ink,FontStyles.Bold);
            Label(card,brief,22,376,w-104,30,19,Ink);
            var tab=Line(card,w-75,333,54,54,Ink);
            // The arrow is printed on each card, and the entire photograph is a deploy target.
            Line(tab,14,26,25,2,Gold);
            var head=Line(tab,30,18,12,2,Gold);head.pivot=Vector2.zero;head.localRotation=Quaternion.Euler(0,0,-43);
            head=Line(tab,30,34,12,2,Gold);head.pivot=Vector2.zero;head.localRotation=Quaternion.Euler(0,0,43);
            return button;
        }

        static Texture2D Texture(string name)=>AssetDatabase.LoadAssetAtPath<Texture2D>(Art+name+".png")
            ??throw new InvalidOperationException("Missing title art "+name);

        static RectTransform Rect(Transform parent,string name,float x,float y,float w,float h)
        {
            var r=(RectTransform)new GameObject(name,typeof(RectTransform)).transform;
            r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);
            r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;
        }

        static RectTransform Line(Transform parent,float x,float y,float w,float h,Color color)
        {
            var r=Rect(parent,"Ink",x,y,w,h);
            var image=r.gameObject.AddComponent<Image>();image.color=color;image.raycastTarget=false;return r;
        }

        static Button Action(Transform parent,string name,float x,float y,float w,float h,Color baseColor)
        {
            var r=Rect(parent,name,x,y,w,h);
            var ink=r.gameObject.AddComponent<Image>();ink.color=Color.white;
            var button=r.gameObject.AddComponent<Button>();button.targetGraphic=ink;
            var colors=button.colors;colors.normalColor=baseColor;
            colors.selectedColor=colors.highlightedColor=baseColor==Ink?DioramaSurface.Hex("4D5D52"):DioramaSurface.Hex("EDCC8F");
            colors.pressedColor=Gold;colors.fadeDuration=.1f;button.colors=colors;return button;
        }

        static void Link(Button from,Button left,Button right,Button vertical)
        {
            var n=from.navigation;n.mode=Navigation.Mode.Explicit;
            n.selectOnLeft=left;n.selectOnRight=right;n.selectOnUp=n.selectOnDown=vertical;from.navigation=n;
        }

        static void Label(Transform parent,string text,float x,float y,float w,float h,int size,Color color,
            FontStyles style=FontStyles.Normal,TextAlignmentOptions alignment=TextAlignmentOptions.MidlineLeft,float spacing=0)
        {
            var label=Rect(parent,text,x,y,w,h).gameObject.AddComponent<TextMeshProUGUI>();
            label.font=TMP_Settings.defaultFontAsset;label.text=text;label.fontSize=size;
            label.fontStyle=style;label.characterSpacing=spacing;label.alignment=alignment;
            label.color=color;label.raycastTarget=false;label.textWrappingMode=TextWrappingModes.NoWrap;
        }
    }
}
