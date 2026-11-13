using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
namespace TheLineHolds
{
    public sealed class GameHud : MonoBehaviour
    {
        public TMP_FontAsset font;
        BattleController game;
        GameObject top,bottom,modal,sell,help,intel,starRow,pauseStatus,options;
        RectTransform dock,dispatch,card;
        TextMeshProUGUI lives,funds,status,notice,waveInfo,selection,selectionStats,modalTitle,modalSubtitle,modalStats;
        UnityEngine.UI.Button sellButton,speedButton,startWave,retry,menu,play,quit,pauseButton,soundButton;
        readonly UnityEngine.UI.Button[] shops=new UnityEngine.UI.Button[3],upgradeButtons=new UnityEngine.UI.Button[2];
        UnityEngine.UI.Button forest;
        readonly StarGraphic[] ratingStars=new StarGraphic[3];
        readonly Color ink=new Color(.075f,.105f,.09f,.96f),paper=new Color(.94f,.92f,.83f),muted=new Color(.71f,.76f,.66f),accent=new Color(.91f,.75f,.39f);
        bool showHelp,showIntel,showOptions;string hint;
        readonly HudIcon[] upgradeLocks=new HudIcon[2];
        public void CloseOptions(){showOptions=false;}
        public void ShowHint(string value){hint=value;}
        static Sprite rounded;
        string lastNotice;float noticeUntil;
        public void Initialize(BattleController controller)
        {
            game=controller;var canvas=gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);scaler.matchWidthOrHeight=.5f;
            gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            if(!EventSystem.current)new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
            top=Group("HUD",transform);
            var resources=Panel("Resources",top.transform,new Vector2(16,-16),new Vector2(355,54));
            Icon(resources.transform,HudSymbol.Heart,new Vector2(16,-15),24,new Color(.9f,.55f,.45f));lives=Label(resources.transform,"20",new Vector2(50,-12),new Vector2(42,30),23,paper);
            funds=Label(resources.transform,"$300",new Vector2(109,-12),new Vector2(105,30),23,paper);
            Icon(resources.transform,HudSymbol.Wave,new Vector2(220,-15),24,muted);status=Label(resources.transform,"0 / 10",new Vector2(254,-12),new Vector2(92,30),21,paper);
            lives.alignment=funds.alignment=status.alignment=TextAlignmentOptions.MidlineLeft;
            var controls=Panel("Quick controls",top.transform,new Vector2(-294,-16),new Vector2(278,54),true);
            speedButton=Button(controls.transform,"1x",new Vector2(4,-4),new Vector2(52,46),()=>game.ToggleSpeed());
            pauseButton=IconButton(controls.transform,"Pause",HudSymbol.Pause,new Vector2(58,-4),()=>game.Pause());
            soundButton=IconButton(controls.transform,"Mute sound",HudSymbol.Sound,new Vector2(112,-4),()=>game.audioPlayer.ToggleMute());
            IconButton(controls.transform,"Help",HudSymbol.Help,new Vector2(166,-4),()=>{showHelp=!showHelp;showOptions=false;});
            IconButton(controls.transform,"Options",HudSymbol.Menu,new Vector2(220,-4),()=>{showOptions=!showOptions;showHelp=false;if(showOptions && !game.Match.Paused)game.Pause();});
            pauseStatus=Panel("Pause status",top.transform,new Vector2(-478,-16),new Vector2(172,54),true);
            Label(pauseStatus.transform,"Paused · edit towers",new Vector2(12,-16),new Vector2(148,25),14,accent);
            options=Panel("Battle options",transform,new Vector2(-240,-82),new Vector2(224,124),true);
            Button(options.transform,"Restart",new Vector2(12,-12),new Vector2(200,44),()=>game.NewMatch());
            Button(options.transform,"Main menu",new Vector2(12,-66),new Vector2(200,44),()=>game.MainMenu());
            bottom=Group("Build dock",transform);dock=(RectTransform)Panel("Build choices",bottom.transform,new Vector2(-202,104),new Vector2(404,88),false,true,.5f).transform;
            string[] names={"MG nest","Anti-tank","Mortar"};var symbols=new[]{HudSymbol.Gun,HudSymbol.Cannon,HudSymbol.Mortar};
            for(int i=0;i<3;i++)
            {
                int type=i;shops[i]=Button(dock,names[i],new Vector2(8+i*132,-6),new Vector2(124,76),()=>game.Choose(type));shops[i].name="Shop "+names[i];
                var shopHint=shops[i].gameObject.AddComponent<HudHint>();shopHint.owner=this;shopHint.hint=controller.defenses[i].description;
                shops[i].GetComponentInChildren<TextMeshProUGUI>().rectTransform.offsetMin=new Vector2(4,3);shops[i].GetComponentInChildren<TextMeshProUGUI>().rectTransform.offsetMax=new Vector2(-4,-38);
                Icon(shops[i].transform,symbols[i],new Vector2(47,-5),30,accent);
            }
            dispatch=(RectTransform)Panel("Wave orders",bottom.transform,new Vector2(-228,104),new Vector2(212,88),true,true).transform;
            startWave=Button(dispatch,"Send wave",new Vector2(8,-8),new Vector2(196,42),()=>game.StartNextWave());
            Button(dispatch,"Wave intel",new Vector2(8,-52),new Vector2(196,28),()=>showIntel=!showIntel).GetComponentInChildren<TextMeshProUGUI>().fontSize=13;
            notice=Label(bottom.transform,"",new Vector2(16,120),new Vector2(650,25),15,paper,false,true);
            intel=Panel("Wave intel",transform,new Vector2(-324,350),new Vector2(308,232),true,true);
            waveInfo=Label(intel.transform,"",new Vector2(18,-18),new Vector2(272,196),16,paper);waveInfo.textWrappingMode=TextWrappingModes.Normal;
            help=Panel("Help",transform,new Vector2(-324,-82),new Vector2(308,260),true);
            Label(help.transform,"Field guide",new Vector2(18,-18),new Vector2(272,24),21,paper);
            string modifier=Application.platform==RuntimePlatform.OSXPlayer || Application.platform==RuntimePlatform.OSXEditor?"Cmd":"Ctrl";
            Label(help.transform,$"Click a clearing to build.\nClick a tower to upgrade.\nClick ground or Esc to deselect.\n\nRotate   MMB / {modifier}+drag / Q E\nMove     WASD / Shift+MMB\nZoom    Mouse wheel",new Vector2(18,-54),new Vector2(272,152),15,muted);
            IconButton(help.transform,"Rotate left",HudSymbol.Left,new Vector2(16,-208),()=>game.CameraControls.Rotate(-45));
            IconButton(help.transform,"Rotate right",HudSymbol.Right,new Vector2(66,-208),()=>game.CameraControls.Rotate(45));
            IconButton(help.transform,"Zoom out",HudSymbol.Minus,new Vector2(116,-208),()=>game.CameraControls.Zoom(1));
            IconButton(help.transform,"Zoom in",HudSymbol.Plus,new Vector2(166,-208),()=>game.CameraControls.Zoom(-1));
            IconButton(help.transform,"Reset view",HudSymbol.Reset,new Vector2(216,-208),()=>game.CameraControls.ResetView());
            sell=Panel("Inspection",transform,new Vector2(-324,-82),new Vector2(308,290),true);
            selection=Label(sell.transform,"",new Vector2(18,-17),new Vector2(230,28),20,paper);
            IconButton(sell.transform,"Close inspection",HudSymbol.Close,new Vector2(258,-6),()=>game.Cancel());
            selectionStats=Label(sell.transform,"",new Vector2(18,-51),new Vector2(272,52),14,muted);
            for(int i=0;i<2;i++){int option=i;upgradeButtons[i]=Button(sell.transform,"Upgrade",new Vector2(18,-110-i*66),new Vector2(272,58),()=>game.UpgradeSelected(option));upgradeButtons[i].name="Upgrade "+i;upgradeLocks[i]=Icon(upgradeButtons[i].transform,HudSymbol.Lock,new Vector2(12,-20),18,muted);}
            sellButton=Button(sell.transform,"Sell",new Vector2(18,-246),new Vector2(272,32),()=>game.SellSelected());sellButton.GetComponentInChildren<TextMeshProUGUI>().fontSize=14;
            modal=Panel("Overlay",transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero,new Color(.025f,.045f,.03f,.48f),true);
            card=(RectTransform)Panel("Dispatch card",modal.transform,new Vector2(-240,210),new Vector2(480,420),false,true,.5f,.5f).transform;
            modalTitle=Label(card,"",new Vector2(32,-32),new Vector2(416,58),36,paper);
            modalSubtitle=Label(card,"",new Vector2(32,-99),new Vector2(416,30),17,accent);
            starRow=Group("Victory stars",card);var sr=(RectTransform)starRow.transform;Place(sr,new Vector2(32,-144),new Vector2(180,40));
            for(int i=0;i<3;i++){var go=new GameObject("Rating star",typeof(RectTransform),typeof(StarGraphic));go.transform.SetParent(sr,false);Place((RectTransform)go.transform,new Vector2(i*50,0),new Vector2(34,34));ratingStars[i]=go.GetComponent<StarGraphic>();ratingStars[i].raycastTarget=false;}
            modalStats=Label(card,"",new Vector2(32,-201),new Vector2(416,76),19,paper);
            play=Button(card,"Countryside",new Vector2(32,-294),new Vector2(200,48),()=>game.Deploy(false));
            forest=Button(card,"Winter Forest",new Vector2(248,-294),new Vector2(200,48),()=>game.Deploy(true));
            retry=Button(card,"Play again",new Vector2(32,-294),new Vector2(416,48),()=>game.NewMatch());
            menu=Button(card,"Main menu",new Vector2(32,-354),new Vector2(200,38),()=>game.MainMenu());
            quit=Button(card,"Quit",new Vector2(248,-354),new Vector2(200,38),Quit);
        }
        public void Refresh()
        {
            var m=game.Match;bool active=!game.InMenu && m!=null;bool editable=active && !m.Ended;
            if(active && !m.Paused)showOptions=false;pauseStatus.SetActive(editable && m.Paused);options.SetActive(editable && showOptions);
            var pauseIcon=pauseButton.GetComponentInChildren<HudIcon>();var symbol=active && m.Paused?HudSymbol.Play:HudSymbol.Pause;if(pauseIcon.symbol!=symbol){pauseIcon.symbol=symbol;pauseIcon.SetVerticesDirty();}pauseButton.GetComponent<HudHint>().hint=active && m.Paused?"Resume":"Pause";
            top.SetActive(active);bottom.SetActive(editable);help.SetActive(editable && showHelp);intel.SetActive(editable && showIntel && !showHelp);
            bool inspect=editable && game.SelectedPad>=0 && m.Towers[game.SelectedPad]!=null;sell.SetActive(inspect && !showHelp && !showOptions);
            if(active)
            {
                lives.text=m.Lives.ToString();funds.text=$"<color=#E8BF64>$</color>{m.Funds}";status.text=$"{m.Wave} / {m.TotalWaves}";
                speedButton.GetComponentInChildren<TextMeshProUGUI>().text=game.GameSpeed+"x";
                var soundIcon=soundButton.GetComponentInChildren<HudIcon>();
                var soundSymbol=game.audioPlayer.Muted?HudSymbol.Muted:HudSymbol.Sound;
                if(soundIcon.symbol!=soundSymbol){soundIcon.symbol=soundSymbol;soundIcon.SetVerticesDirty();}
                soundButton.GetComponent<HudHint>().hint=game.audioPlayer.Muted?"Unmute sound":"Mute sound";
                if(lastNotice!=game.Notice){lastNotice=game.Notice;noticeUntil=Time.unscaledTime+3.2f;}
                notice.text=hint??(Time.unscaledTime<noticeUntil?ShortNotice(game.Notice):"");
                var next=m.NextWave;string roster=string.Join("\n",System.Array.ConvertAll(next.spawns,e=>$"{e.UnitCount}  {e.enemy.displayName}"));
                waveInfo.text=$"<b>{next.title}</b>\n<size=13>{next.formation}</size>\n{roster}";
                bool prep=m.Phase==MatchPhase.Preparation;startWave.interactable=prep && !m.Paused;
                startWave.GetComponentInChildren<TextMeshProUGUI>().text=m.Paused?(prep?"Resume to send":"Wave paused"):prep?$"Send wave {Mathf.Min(m.Wave+1,m.TotalWaves)}":"Wave underway";
                for(int i=0;i<3;i++)
                {
                    var d=game.defenses[i];shops[i].interactable=editable;
                    shops[i].GetComponentInChildren<TextMeshProUGUI>().text=$"{new[]{"MG nest","Anti-tank","Mortar"}[i]}  <color=#E8BF64>${d.cost}</color>";
                    shops[i].GetComponent<UnityEngine.UI.Image>().color=game.Placement==i?new Color(.35f,.41f,.27f):m.Funds<d.cost?new Color(.12f,.16f,.13f):new Color(.19f,.25f,.2f);
                }
                if(inspect)
                {
                    var t=m.Towers[game.SelectedPad];var d=t.Definition;selection.text=ShortName(d);selectionStats.text=$"Tier {d.tier}   •   {d.damage:0} damage   •   {d.range:0.0}m range\n{RoleHint(d)}";
                    float y=110+d.upgrades.Length*66;((RectTransform)sell.transform).sizeDelta=new Vector2(308,y+54);Place((RectTransform)sellButton.transform,new Vector2(18,-y),new Vector2(272,32));
                    sellButton.GetComponentInChildren<TextMeshProUGUI>().text=$"Sell  +${t.SellValue}";
                    for(int i=0;i<2;i++)
                    {
                        bool exists=i<d.upgrades.Length;var button=upgradeButtons[i];button.gameObject.SetActive(exists);if(!exists)continue;
                        var u=d.upgrades[i];bool affordable=m.Funds>=u.price;button.interactable=affordable;upgradeLocks[i].gameObject.SetActive(!affordable);
                        var colors=button.colors;colors.disabledColor=Color.white;button.colors=colors;
                        button.GetComponent<UnityEngine.UI.Image>().color=affordable?new Color(.23f,.31f,.24f):new Color(.12f,.15f,.13f);
                        var text=button.GetComponentInChildren<TextMeshProUGUI>();text.color=affordable?paper:new Color(.49f,.55f,.48f);
                        text.text=affordable?$"{ShortName(u.defense)}   <color=#E8BF64>${u.price}</color>\n<size=12>{RoleHint(u.defense)}</size>":$"{ShortName(u.defense)}   ${u.price}\n<size=12>Need ${u.price-m.Funds} more</size>";
                    }
                }
            }
            bool show=game.InMenu || active && m.Ended;modal.SetActive(show);if(!show)return;
            bool end=active && m.Ended;play.gameObject.SetActive(game.InMenu);forest.gameObject.SetActive(game.InMenu);retry.gameObject.SetActive(end);menu.gameObject.SetActive(active);quit.gameObject.SetActive(game.InMenu);
            starRow.SetActive(end && m.Stars>0);for(int i=0;i<3;i++)ratingStars[i].color=end && i<m.Stars?accent:new Color(.27f,.33f,.28f);
            if(game.InMenu){modalTitle.text="The Line Holds";modalSubtitle.text="Choose your battlefield";modalStats.text="Normandy farm road or forest signal post.\nHold the line through ten attacks.";}
            else if(end){modalTitle.text=m.Phase==MatchPhase.Victory?"The line held.":"Regroup.";modalSubtitle.text=m.Phase==MatchPhase.Victory?"Victory":"The defenses fell";modalStats.text=$"{m.Lives} lives saved     {m.CompletedWaves} / {m.TotalWaves} waves\n{m.Killed} defeated     {m.Escaped} escaped";}
        }
        static string ShortName(DefenseDefinition d)=>d.tier==1?(d.role==DefenseRole.MachineGun?"MG nest":d.role==DefenseRole.Cannon?"Anti-tank":"Mortar"):d.displayName.Replace("machine gun","MG").Replace("anti-tank","gun").Replace(" nest","");
        static string RoleHint(DefenseDefinition d)=>d.slowDuration>0?"Suppresses movement":d.role==DefenseRole.Mortar?"Area bombardment":d.splashRadius>0?"Explosive shells":d.role==DefenseRole.Cannon?"Armor specialist":d.armorDamageMultiplier>.5f?"Pierces light armor":"Infantry specialist";
        static string ShortNotice(string s){if(string.IsNullOrEmpty(s))return "";int i=s.IndexOf('.');return i>=0?s.Substring(0,i+1):s;}
        void Quit(){
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying=false;
#else
            Application.Quit();
#endif
        }
        GameObject Group(string name,Transform parent){var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);var r=(RectTransform)go.transform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;return go;}
        GameObject Panel(string name,Transform parent,Vector2 pos,Vector2 size,bool right=false,bool bottom=false,float ax=-1,float ay=-1)
        {
            var go=Panel(name,parent,Vector2.zero,Vector2.zero,Vector2.zero,Vector2.zero,ink,true);var r=(RectTransform)go.transform;r.anchorMin=r.anchorMax=new Vector2(ax>=0?ax:right?1:0,ay>=0?ay:bottom?0:1);r.pivot=new Vector2(0,1);r.anchoredPosition=pos;r.sizeDelta=size;return go;
        }
        GameObject Panel(string name,Transform parent,Vector2 min,Vector2 max,Vector2 low,Vector2 high,Color color,bool blocks)
        {var go=new GameObject(name,typeof(RectTransform),typeof(UnityEngine.UI.Image));go.transform.SetParent(parent,false);var r=(RectTransform)go.transform;r.anchorMin=min;r.anchorMax=max;r.offsetMin=low;r.offsetMax=high;var image=go.GetComponent<UnityEngine.UI.Image>();image.color=color;image.raycastTarget=blocks;
            if(name!="Overlay"){image.sprite=Rounded();image.type=UnityEngine.UI.Image.Type.Sliced;}return go;}
        static Sprite Rounded()
        {
            if(rounded)return rounded;const int n=32;var texture=new Texture2D(n,n,TextureFormat.RGBA32,false);texture.name="HUD soft corners";
            for(int y=0;y<n;y++)for(int x=0;x<n;x++){var q=new Vector2(Mathf.Max(0,8-Mathf.Min(x,n-1-x)),Mathf.Max(0,8-Mathf.Min(y,n-1-y)));texture.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(8.5f-q.magnitude)));}texture.Apply();
            rounded=Sprite.Create(texture,new Rect(0,0,n,n),Vector2.one*.5f,100,0,SpriteMeshType.FullRect,new Vector4(9,9,9,9));return rounded;
        }
        static void Place(RectTransform r,Vector2 p,Vector2 s){r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=p;r.sizeDelta=s;}
        TextMeshProUGUI Label(Transform parent,string text,Vector2 pos,Vector2 size,int fontSize,Color color,bool right=false,bool bottom=false)
        {
            var go=new GameObject("Label",typeof(RectTransform),typeof(TextMeshProUGUI));go.transform.SetParent(parent,false);var r=(RectTransform)go.transform;Place(r,pos,size);r.anchorMin=r.anchorMax=new Vector2(right?1:0,bottom?0:1);var l=go.GetComponent<TextMeshProUGUI>();l.font=font;l.text=text;l.fontSize=fontSize;l.color=color;l.raycastTarget=false;l.textWrappingMode=TextWrappingModes.NoWrap;l.overflowMode=TextOverflowModes.Ellipsis;return l;
        }
        HudIcon Icon(Transform parent,HudSymbol symbol,Vector2 pos,float size,Color color){var go=new GameObject(symbol.ToString(),typeof(RectTransform),typeof(HudIcon));go.transform.SetParent(parent,false);Place((RectTransform)go.transform,pos,Vector2.one*size);var i=go.GetComponent<HudIcon>();i.symbol=symbol;i.color=color;i.raycastTarget=false;return i;}
        UnityEngine.UI.Button IconButton(Transform parent,string name,HudSymbol symbol,Vector2 pos,UnityAction action){var b=Button(parent,"",pos,new Vector2(46,46),action);b.name=name;var hint=b.gameObject.AddComponent<HudHint>();hint.owner=this;hint.hint=name;Icon(b.transform,symbol,new Vector2(12,-12),22,paper);return b;}
        UnityEngine.UI.Button Button(Transform parent,string text,Vector2 pos,Vector2 size,UnityAction action)
        {
            var go=Panel(text,parent,pos,size);var b=go.AddComponent<UnityEngine.UI.Button>();var image=go.GetComponent<UnityEngine.UI.Image>();image.color=new Color(.19f,.25f,.2f);b.targetGraphic=image;b.onClick.AddListener(action);var colors=b.colors;colors.highlightedColor=new Color(1.3f,1.3f,1.18f);colors.pressedColor=new Color(.8f,.88f,.72f);colors.disabledColor=new Color(.48f,.55f,.48f);b.colors=colors;
            var l=Label(go.transform,text,Vector2.zero,size,16,paper);l.alignment=TextAlignmentOptions.Center;l.rectTransform.anchorMin=Vector2.zero;l.rectTransform.anchorMax=Vector2.one;l.rectTransform.offsetMin=new Vector2(8,2);l.rectTransform.offsetMax=new Vector2(-8,-2);return b;
        }
    }
}
