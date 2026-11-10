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
        GameObject top, bottom, modal, sell, help, starRow, viewControls;
        readonly StarGraphic[] ratingStars=new StarGraphic[3];
        TextMeshProUGUI status, notice, waveInfo, selection, modalTitle, modalSubtitle, modalStats, brandTitle, brandSubtitle, cameraHelp;
        float layoutWidth=-1;
        UnityEngine.UI.Button sellButton, speedButton, startWave, resume, retry, menu, play, quit;
        readonly UnityEngine.UI.Button[] upgradeButtons=new UnityEngine.UI.Button[2];
        readonly UnityEngine.UI.Button[] shops=new UnityEngine.UI.Button[3];
        readonly Color ink=new Color(.085f,.105f,.095f,.97f), paper=new Color(.88f,.85f,.73f), accent=new Color(.78f,.67f,.36f);
        public void Initialize(BattleController controller)
        {
            game=controller;
            var canvas=gameObject.AddComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=gameObject.AddComponent<UnityEngine.UI.CanvasScaler>(); scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1600,900); scaler.matchWidthOrHeight=.5f;
            gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            if(!EventSystem.current) { var events=new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule)); }
            top=Panel("Command header",transform,new Vector2(0,1),new Vector2(1,1),new Vector2(0,-98),Vector2.zero,ink);
            brandTitle=Label(top.transform,"THE LINE HOLDS",new Vector2(24,-14),new Vector2(300,32),26,accent);
            brandSubtitle=Label(top.transform,"NORMANDY  /  FIELD OPERATIONS  /  1944",new Vector2(25,-53),new Vector2(430,24),13,paper);
            status=Label(top.transform,"",new Vector2(490,-22),new Vector2(810,44),23,paper);
            speedButton=Button(top.transform,"1x",new Vector2(-242,-23),new Vector2(80,48),()=>game.ToggleSpeed(),true);
            Button(top.transform,"II   PAUSE",new Vector2(-150,-23),new Vector2(130,48),()=>game.Pause(),true);
            bottom=Panel("Supply bar",transform,Vector2.zero,new Vector2(1,0),Vector2.zero,new Vector2(0,180),ink);
            for(int i=0;i<3;i++) { int type=i; shops[i]=Button(bottom.transform,"",new Vector2(24+i*285,22),new Vector2(270,73),()=>game.Choose(type),false,true); shops[i].name="Shop "+controller.defenses[i].displayName; }
            startWave=Button(bottom.transform,"START WAVE  1  >",new Vector2(-280,28),new Vector2(256,68),()=>game.StartNextWave(),true,true);
            notice=Label(bottom.transform,"",new Vector2(24,104),new Vector2(1140,32),17,paper,true);
            waveInfo=Label(bottom.transform,"",new Vector2(900,20),new Vector2(365,75),16,paper,true);
            viewControls=Panel("Map controls",transform,Vector2.zero,new Vector2(1,0),new Vector2(24,190),new Vector2(-24,228),new Color(.08f,.1f,.09f,.9f));
            string modifier=Application.platform==RuntimePlatform.OSXPlayer || Application.platform==RuntimePlatform.OSXEditor?"Cmd":"Ctrl";
            cameraHelp=Label(viewControls.transform,$"Rotate: MMB / {modifier}+drag / Q E   Pan: Shift+MMB / WASD   Zoom: wheel   Reset: R",new Vector2(12,-8),new Vector2(1030,24),13,paper);
            Button(viewControls.transform,"<",new Vector2(-360,-3),new Vector2(44,32),()=>game.CameraControls.Rotate(-45),true);
            Button(viewControls.transform,">",new Vector2(-310,-3),new Vector2(44,32),()=>game.CameraControls.Rotate(45),true);
            Button(viewControls.transform,"-",new Vector2(-250,-3),new Vector2(44,32),()=>game.CameraControls.Zoom(1),true);
            Button(viewControls.transform,"+",new Vector2(-200,-3),new Vector2(44,32),()=>game.CameraControls.Zoom(-1),true);
            var resetView=Button(viewControls.transform,"RESET VIEW",new Vector2(-144,-3),new Vector2(132,32),()=>game.CameraControls.ResetView(),true);
            resetView.GetComponentInChildren<TextMeshProUGUI>().fontSize=14;
            help=Panel("Field guide",transform,new Vector2(0,1),new Vector2(0,1),new Vector2(24,-222),new Vector2(399,-110),new Color(.08f,.1f,.09f,.86f));
            Label(help.transform,"HOLD THE ROAD",new Vector2(16,-12),new Vector2(340,25),18,accent);
            Label(help.transform,"Build on clearings. Click towers to upgrade.\nAmber bars: suppressed enemies.\nRight-click: cancel / Esc: pause",new Vector2(16,-44),new Vector2(345,59),14,paper);
            sell=Panel("Inspection",transform,new Vector2(1,1),new Vector2(1,1),new Vector2(-360,-485),new Vector2(-24,-120),ink);
            selection=Label(sell.transform,"",new Vector2(18,-12),new Vector2(300,95),16,paper);
            for(int i=0;i<2;i++){int option=i;upgradeButtons[i]=Button(sell.transform,"UPGRADE",new Vector2(18,-120-i*83),new Vector2(298,76),()=>game.UpgradeSelected(option));var text=upgradeButtons[i].GetComponentInChildren<TextMeshProUGUI>();text.fontSize=14;text.enableAutoSizing=true;text.fontSizeMin=11;text.fontSizeMax=15;text.textWrappingMode=TextWrappingModes.Normal;}
            sellButton=Button(sell.transform,"SELL",new Vector2(18,-310),new Vector2(298,38),()=>game.SellSelected());
            modal=Panel("Overlay",transform,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero,new Color(.04f,.055f,.045f,.66f),true);
            var card=Panel("Dispatch card",modal.transform,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(-330,-300),new Vector2(330,300),ink);
            Label(card.transform,"T H E   L I N E   H O L D S",new Vector2(42,-30),new Vector2(580,28),17,accent);
            modalTitle=Label(card.transform,"",new Vector2(42,-92),new Vector2(580,75),45,paper);
            modalSubtitle=Label(card.transform,"",new Vector2(42,-183),new Vector2(570,87),19,accent);
            starRow=new GameObject("Victory stars",typeof(RectTransform)); starRow.transform.SetParent(card.transform,false);
            var starRect=(RectTransform)starRow.transform; starRect.anchorMin=starRect.anchorMax=new Vector2(0,1); starRect.pivot=new Vector2(0,1); starRect.anchoredPosition=new Vector2(42,-183); starRect.sizeDelta=new Vector2(165,42);
            for(int i=0;i<3;i++)
            {
                var star=new GameObject("Rating star "+(i+1),typeof(RectTransform),typeof(StarGraphic)); star.transform.SetParent(starRow.transform,false);
                var rect=(RectTransform)star.transform; rect.anchorMin=rect.anchorMax=new Vector2(0,1); rect.pivot=new Vector2(0,1); rect.anchoredPosition=new Vector2(i*54,0); rect.sizeDelta=new Vector2(40,40);
                ratingStars[i]=star.GetComponent<StarGraphic>(); ratingStars[i].raycastTarget=false;
            }
            modalStats=Label(card.transform,"",new Vector2(42,-280),new Vector2(570,95),18,paper);
            play=Button(card.transform,"DEPLOY   >",new Vector2(42,-405),new Vector2(570,57),()=>game.NewMatch());
            resume=Button(card.transform,"RESUME",new Vector2(42,-405),new Vector2(570,57),()=>game.Pause());
            retry=Button(card.transform,"RETRY",new Vector2(42,-405),new Vector2(570,57),()=>game.NewMatch());
            menu=Button(card.transform,"MAIN MENU",new Vector2(42,-480),new Vector2(570,45),()=>game.MainMenu());
            quit=Button(card.transform,"QUIT",new Vector2(42,-480),new Vector2(570,45),Quit);
        }
        public void Refresh()
        {
            LayoutHud();
            var m=game.Match; bool inMatch=!game.InMenu && m!=null;
            viewControls.SetActive(inMatch && !m.Paused && !m.Ended);
            top.SetActive(inMatch); bottom.SetActive(inMatch); help.SetActive(inMatch && m.Wave==0 && !m.Paused);
            bool inspect=inMatch && game.SelectedPad>=0 && m.Towers[game.SelectedPad]!=null && !m.Ended && !m.Paused;
            sell.SetActive(inspect);
            if(inspect)
            {
                var tower=m.Towers[game.SelectedPad];var d=tower.Definition;
                selection.text=$"{d.displayName.ToUpper()}  ·  TIER {d.tier}\n{d.damage:0} damage / {1/d.interval:0.0} shots/s / {d.range:0.0} m\nSell ${tower.SellValue} of ${tower.Invested} invested";
                if(d.upgrades.Length==0)selection.text+="\nSPECIALIZATION COMPLETE";
                float sellY=d.upgrades.Length==0?125:d.upgrades.Length==1?215:310;
                ((RectTransform)sellButton.transform).anchoredPosition=new Vector2(18,-sellY);
                ((RectTransform)sell.transform).offsetMin=new Vector2(-360,-120-sellY-55);
                for(int i=0;i<2;i++)
                {
                    bool exists=i<d.upgrades.Length;upgradeButtons[i].gameObject.SetActive(exists);if(!exists)continue;
                    var u=d.upgrades[i];upgradeButtons[i].interactable=m.Funds>=u.price;
                    upgradeButtons[i].GetComponentInChildren<TextMeshProUGUI>().text=$"{u.defense.displayName.ToUpper()}  ${u.price}\n<size=12>{u.defense.description}</size>";
                }
            }
            if(inMatch)
            {
                string divider=layoutWidth<1500?"\n":"     •     ";
                speedButton.GetComponentInChildren<TextMeshProUGUI>().text=game.GameSpeed+"x";
                status.text=$"LIVES  {m.Lives:00}     •     FUNDS  ${m.Funds}{divider}WAVE  {m.Wave} / {m.TotalWaves}     •     HOSTILES  {m.Remaining}";
                notice.text=game.Notice;
                var next=m.NextWave;
                string roster=string.Join(" · ",System.Array.ConvertAll(next.spawns,e=>$"{e.UnitCount} {e.enemy.displayName}"));
                waveInfo.text=m.Phase==MatchPhase.Preparation?$"NEXT {Mathf.Min(m.Wave+1,m.TotalWaves)}: {next.title.ToUpper()} · {next.formation}\n{roster}\n{next.briefing}":$"{game.waves[m.Wave-1].title.ToUpper()} · {game.waves[m.Wave-1].formation}\nBuild, upgrade and sell during combat. Click a tower to inspect it.";
                startWave.interactable=m.Phase==MatchPhase.Preparation && !m.Paused;
                startWave.GetComponentInChildren<TextMeshProUGUI>().text=m.Phase==MatchPhase.Combat?"WAVE IN PROGRESS":$"START WAVE  {Mathf.Min(m.Wave+1,m.TotalWaves)}  >";
                for(int i=0;i<3;i++)
                {
                    var d=game.defenses[i]; shops[i].interactable=!m.Ended && !m.Paused;
                    shops[i].GetComponentInChildren<TextMeshProUGUI>().text=$"{(game.Placement==i?"> ":"")}{d.displayName.ToUpper()}   ${d.cost}\n<size=14>{d.description}</size>";
                    shops[i].GetComponent<UnityEngine.UI.Image>().color=game.Placement==i?new Color(.38f,.38f,.23f):m.Funds<d.cost?new Color(.22f,.23f,.2f):new Color(.25f,.29f,.23f);
                }
            }
            bool show=game.InMenu || (m!=null && (m.Paused || m.Ended)); modal.SetActive(show);
            if(!show) return;
            play.gameObject.SetActive(game.InMenu); quit.gameObject.SetActive(game.InMenu);
            resume.gameObject.SetActive(!game.InMenu && m.Paused && !m.Ended);
            retry.gameObject.SetActive(!game.InMenu && m.Ended); menu.gameObject.SetActive(!game.InMenu);
            // Pause offers restart as well as resume and menu.
            if(!game.InMenu && m.Paused && !m.Ended)
            {
                retry.gameObject.SetActive(true); var r=(RectTransform)retry.transform; r.anchoredPosition=new Vector2(42,-474); r.sizeDelta=new Vector2(275,43);
                var mr=(RectTransform)menu.transform; mr.anchoredPosition=new Vector2(337,-474); mr.sizeDelta=new Vector2(275,43);
                retry.GetComponentInChildren<TextMeshProUGUI>().text="RESTART";
            }
            else
            {
                var r=(RectTransform)retry.transform; r.anchoredPosition=new Vector2(42,-405); r.sizeDelta=new Vector2(570,57);
                var mr=(RectTransform)menu.transform; mr.anchoredPosition=new Vector2(42,-480); mr.sizeDelta=new Vector2(570,45);
                retry.GetComponentInChildren<TextMeshProUGUI>().text="RETRY";
            }
            bool showStars=!game.InMenu && m.Ended && m.Stars>0;
            starRow.SetActive(showStars);
            ((RectTransform)modalSubtitle.transform).anchoredPosition=new Vector2(42,showStars?-235:-183);
            for(int i=0;i<3;i++)ratingStars[i].color=showStars && i<m.Stars?accent:new Color(.28f,.3f,.25f);
            if(game.InMenu)
            {
                modalTitle.text="A road worth holding.";
                modalSubtitle.text="One country road. Ten varied attacks.\nEvery emplacement matters.";
                modalStats.text="Place defenses • Protect the exit • Hold all ten waves\nMachine guns for infantry. Anti-tank for armor.\nUpgrade towers and choose specializations.";
            }
            else if(m.Ended)
            {
                modalTitle.text=m.Phase==MatchPhase.Victory?"THE LINE HELD.":"THE LINE BROKE.";
                modalSubtitle.text=m.Phase==MatchPhase.Victory?$"VICTORY   /   {m.Stars} OF 3 STARS":"DEFEAT   /   REGROUP AND TRY AGAIN";
                modalStats.text=$"LIVES REMAINING   {m.Lives} / 20      WAVES HELD   {m.CompletedWaves} / {m.TotalWaves}\nENEMIES KILLED    {m.Killed}       ESCAPED    {m.Escaped}\n18+ lives: 3 stars  •  10–17: 2 stars  •  1–9: 1 star";
            }
            else { modalTitle.text="ORDERS ON HOLD."; modalSubtitle.text="Combat is paused."; modalStats.text="Take a breath. Inspect the battlefield.\nResume when you are ready."; }
        }
        // Reserve disjoint regions at every canvas width, including tall/narrow resized windows.
        void LayoutHud()
        {
            float width=((RectTransform)transform).rect.width;
            if(width<=0 || Mathf.Abs(width-layoutWidth)<.5f)return;
            layoutWidth=width;
            float headerLeft=Mathf.Clamp(width*.3f,240,450);
            brandTitle.rectTransform.sizeDelta=new Vector2(headerLeft-48,32);
            brandSubtitle.rectTransform.sizeDelta=new Vector2(headerLeft-48,24);
            Fit(brandTitle,16,26);Fit(brandSubtitle,9,13);
            var sr=status.rectTransform;sr.anchorMin=new Vector2(0,1);sr.anchorMax=Vector2.one;
            sr.offsetMin=new Vector2(headerLeft,-82);sr.offsetMax=new Vector2(-255,-14);
            status.alignment=TextAlignmentOptions.MidlineLeft;Fit(status,16,23);
            float column=(width-48-36)/4;
            for(int i=0;i<3;i++)
            {
                Place((RectTransform)shops[i].transform,new Vector2(24+i*(column+12),16),new Vector2(column,68));
                var label=shops[i].GetComponentInChildren<TextMeshProUGUI>();label.enableAutoSizing=true;label.fontSizeMin=13;label.fontSizeMax=18;
            }
            Place((RectTransform)startWave.transform,new Vector2(24+3*(column+12),16),new Vector2(column,68));
            Fit(startWave.GetComponentInChildren<TextMeshProUGUI>(),13,18);
            Place(notice.rectTransform,new Vector2(24,148),new Vector2(width-48,26));Fit(notice,13,17);
            Place(waveInfo.rectTransform,new Vector2(24,88),new Vector2(width-48,58));
            waveInfo.enableAutoSizing=true;waveInfo.fontSizeMin=11;waveInfo.fontSizeMax=14;waveInfo.textWrappingMode=TextWrappingModes.NoWrap;
            cameraHelp.rectTransform.sizeDelta=new Vector2(Mathf.Max(100,width-440),24);Fit(cameraHelp,10,13);
        }
        static void Fit(TextMeshProUGUI label,int minimum,int maximum)
        {
            label.enableAutoSizing=true;label.fontSizeMin=minimum;label.fontSizeMax=maximum;
            label.textWrappingMode=TextWrappingModes.NoWrap;label.overflowMode=TextOverflowModes.Ellipsis;
        }
        static void Place(RectTransform rect,Vector2 position,Vector2 size)
        {
            rect.anchorMin=rect.anchorMax=rect.pivot=Vector2.zero;
            rect.anchoredPosition=position;rect.sizeDelta=size;
        }
        void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying=false;
#else
            Application.Quit();
#endif
        }
        GameObject Panel(string name,Transform parent,Vector2 min,Vector2 max,Vector2 offsetMin,Vector2 offsetMax,Color color,bool blocks=false)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(UnityEngine.UI.Image)); go.transform.SetParent(parent,false);
            var rt=(RectTransform)go.transform; rt.anchorMin=min; rt.anchorMax=max; rt.offsetMin=offsetMin; rt.offsetMax=offsetMax;
            go.GetComponent<UnityEngine.UI.Image>().color=color; go.GetComponent<UnityEngine.UI.Image>().raycastTarget=blocks; return go;
        }
        TextMeshProUGUI Label(Transform parent,string text,Vector2 pos,Vector2 size,int fontSize,Color color,bool fromBottom=false)
        {
            var go=new GameObject("Label",typeof(RectTransform),typeof(TextMeshProUGUI)); go.transform.SetParent(parent,false);
            var rt=(RectTransform)go.transform; rt.anchorMin=rt.anchorMax=new Vector2(0,fromBottom?0:1); rt.pivot=new Vector2(0,fromBottom?0:1); rt.anchoredPosition=pos; rt.sizeDelta=size;
            var label=go.GetComponent<TextMeshProUGUI>(); label.font=font; label.text=text; label.fontSize=fontSize; label.color=color; label.raycastTarget=false; label.textWrappingMode=TextWrappingModes.Normal; return label;
        }
        UnityEngine.UI.Button Button(Transform parent,string text,Vector2 pos,Vector2 size,UnityAction action,bool right=false,bool fromBottom=false)
        {
            var go=new GameObject(text,typeof(RectTransform),typeof(UnityEngine.UI.Image),typeof(UnityEngine.UI.Button)); go.transform.SetParent(parent,false);
            var rt=(RectTransform)go.transform; rt.anchorMin=rt.anchorMax=new Vector2(right?1:0,fromBottom?0:1); rt.pivot=new Vector2(0,fromBottom?0:1); rt.anchoredPosition=pos; rt.sizeDelta=size;
            go.GetComponent<UnityEngine.UI.Image>().color=new Color(.25f,.29f,.23f);
            var b=go.GetComponent<UnityEngine.UI.Button>(); b.targetGraphic=go.GetComponent<UnityEngine.UI.Image>(); b.onClick.AddListener(action);
            var colors=b.colors; colors.highlightedColor=new Color(1.25f,1.2f,1.05f); colors.disabledColor=new Color(.5f,.5f,.5f); b.colors=colors;
            var label=Label(go.transform,text,Vector2.zero,size,18,paper); var lr=(RectTransform)label.transform; lr.anchorMin=Vector2.zero; lr.anchorMax=Vector2.one; lr.offsetMin=new Vector2(12,2); lr.offsetMax=new Vector2(-12,-2); label.alignment=TextAlignmentOptions.Center;
            return b;
        }
    }
}
