using UnityEngine;

namespace TheLineHolds.Editor
{
    // Each frontage runs along one street. Widths are party-wall to party-wall,
    // independent of roof overhangs. Pairs of frontages enclose the rear courts.
    internal sealed class TownBlocks
    {
        readonly TownScenery scenery;
        internal TownBlocks(TownScenery scenery) => this.scenery=scenery;

        internal void Build()
        {
            Row("Gate street", -9.38f,5.14f,0,1.3f, .83f,("RuinFacade",1.58f), ("Maison",1.62f));
            Row("Gate warehouse court", -11.5f,-5.68f,0,1.85f,1.03f,("BrickTenement",2.62f), ("BurnedHouse",1.72f), ("Maison",2.15f));
            Row("West side of the dogleg", -6.97f,6.88f,0,1.55f,.79f,("GabledDraper",1.82f), ("Mercerie",2.06f));
            Row("Drapers' inner island", -.29f,3.06f,0,1.42f,.85f,("Maison",1.80f), ("GabledDraper",1.51f));
            Row("East side of customs street", 6.64f,.08f,0,1.46f,.89f,("RuinFacade",1.65f), ("Boulangerie",1.76f));
            Row("Western market frontage", -4.63f,8.85f,0,1.88f,.94f,("ShellHouse",1.82f), ("Hotel",2.74f), ("GabledDraper",1.95f), ("Mercerie",2.03f));
            Row("Station street", -2.16f,-6.08f,0,2.02f,.88f,("BrickTenement",2.57f), ("RuinFacade",1.71f), ("Maison",2.16f));
            Row("Station court return", -3.09f,-3.46f,90,1.66f,.82f,("Mercerie",1.88f), ("ShellHouse",1.55f));
            Row("Southern quayside", 3.61f,-8.24f,0,2.03f,.91f,("Hotel",2.65f), ("Boulangerie",2.01f), ("BrickTenement",2.18f));
            Row("East boulevard", 13.05f,4.65f,90,1.86f,1.01f,("Hotel",2.72f), ("Arcade",2.26f), ("Maison",2.12f));
            Row("East boulevard south",13.67f,-5.89f,90,1.93f,.88f,("BurnedHouse",1.71f), ("GabledDraper",1.81f), ("Hotel",2.62f));

            Row("Drapers' return",.77f,1.13f,90,1.16f,.86f,("BrickTenement",1.49f), ("ShellHouse",1.43f));
            Row("Drapers' rear court",-.81f,-1.74f,0,1.04f,.72f,("RuinFacade",1.52f), ("Maison",1.46f));
            Row("Customs return",7.83f,-2.01f,90,1.10f,.78f,("Maison",1.35f), ("RuinFacade",1.28f));
            Row("Gate terrace",-14.26f,6.14f,0,1.63f,.91f,("Mercerie",2.26f), ("Maison",2.03f), ("RuinFacade",1.72f));
            Row("East boulevard carriage court",15.32f,7.02f,0,1.48f,.94f,("Boulangerie",2.02f), ("GabledDraper",2.07f));
            Row("Canal terrace",20.4f,2.7f,90,1.99f,1.08f,("Mercerie",2.15f), ("ShellHouse",1.85f), ("BrickTenement",2.61f), ("Hotel",2.47f));
            Row("Canal court",20.1f,-6.3f,0,1.97f,.96f,("Boulangerie",2.04f), ("Maison",2.25f), ("RuinFacade",2.21f));
            scenery.Place("ParishChurch",1.21f,10.95f,new Vector3(1.12f,1.05f,1.1f));
            Row("Church close",1.51f,14.10f,0,1.72f,1.06f,("Maison",1.8f), ("Hotel",2.83f), ("RuinFacade",1.69f), ("Boulangerie",2.1f));
            Row("Cathedral mews",-9.25f,15.05f,0,1.74f,1.03f,("Maison",2.45f), ("Arcade",2.22f), ("BrickTenement",2.57f));
            Row("Foundry street",13.03f,-10.62f,0,1.78f,.97f,("RuinFacade",1.58f), ("Maison",1.94f));
            Row("Lower gardens",-.1f,-14.02f,0,1.9f,.99f,("Hotel",3.13f), ("Arcade",2.25f), ("Mercerie",2.07f), ("Maison",1.95f));
            // Civic ensemble: the market gallery continues directly into the west wing.
            scenery.Place("Mairie",6.66f,6.39f,new Vector3(1.48f,1.21f,1.20f),180);
            Row("Market gallery",11.03f,6.68f,180,1.60f,.95f,("Arcade",2.42f), ("Arcade",2.42f));
            Row("Rear of the town hall",7.65f,10.33f,180,1.98f,1.12f,("Hotel",3.18f), ("Maison",2.16f), ("BrickTenement",2.70f));
            Row("Garden court", -3.88f,11.66f,180,2.03f,1.10f,("Maison",2.2f), ("BrickTenement",3.04f), ("Hotel",2.86f));
            Row("Chapel lane", -10.38f,10.18f,90,1.82f,1.04f,("GabledDraper",2.28f), ("RuinFacade",1.97f), ("Maison",2.04f));
            Row("Rue des Tanneurs", -8.05f,-8.40f,0,1.85f,.9f,("Arcade",2.39f), ("Boulangerie",2.10f), ("ShellHouse",2.33f), ("Maison",1.83f));
            Row("Rue des Tanneurs return",-12.11f,-10.25f,90,1.77f,1.06f,("Maison",1.85f), ("Hotel",2.65f));
            Row("South square",-.65f,-10.87f,0,1.93f,1.02f,("Maison",1.84f), ("Hotel",3.15f), ("Mercerie",2.03f));
            Row("Tramway terrace",8.17f,-11.24f,180,1.96f,1.06f,("Maison",2.04f), ("Arcade",2.4f), ("RuinFacade",1.92f), ("GabledDraper",2.38f));
            Row("Quai east court",17.24f,-1.85f,90,2.04f,1.12f,("Maison",2.14f), ("Hotel",2.95f), ("GabledDraper",1.97f), ("BrickTenement",2.48f));
            Row("Quai north court",16.50f,10.80f,90,2.02f,1.16f,("BrickTenement",2.66f), ("Maison",2.12f), ("Mercerie",2.01f));
        }

        void Row(string street,float x,float z,float yaw,float depth,float height,params (string Model,float Width)[] lots)
        {
            float frontage=0;
            foreach(var lot in lots)frontage+=lot.Width;
            Vector3 along=Quaternion.Euler(0,yaw,0)*Vector3.right;
            float cursor=-frontage/2;
            for(int i=0;i<lots.Length;i++)
            {
                var (model,width)=lots[i];
                var p=new Vector3(x,0,z)+along*(cursor+width/2);
                bool oldKit=model is "Maison" or "Mercerie" or "Boulangerie" or "ShellHouse" or "BurnedHouse";
                float storeyScale=height*(1+.06f*Mathf.Sin(i*3.7f+x));
                var instance=scenery.Place(model,p.x,p.z,new Vector3(width/2.4f,storeyScale,depth/(oldKit?1.52f:1.72f)),yaw);
                instance.name=street+" · "+model;
                cursor+=width;
            }
        }
    }
}
