using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Platform;

namespace Overdraw_Bug {
    public partial class App : Application {
        public App() {
            InitializeComponent();

            //=====================
            //DEMO CONTROLS
            //=====================
            bool drawBackground = false;
            bool addMoreItems = true; 
            bool animateOverElement = false;
            Type typeToAdd = typeof(AbsoluteLayout); //change type to add here (Border/BoxView increase overdraw even when empty, Layout/Image do not)
            bool colorizeExtras = true; //add color to Bg elements or not
            int numToAdd = 1;

            //=====================
            //BUILD PAGE
            //=====================
            ContentPage mainPage = new();
            if (!drawBackground) {
                mainPage.BackgroundColor = null;
            }
            mainPage.BackgroundColor = Colors.Black; //NOTE THAT IF YOU MANUALLY SET THIS HERE AND THEN LATER (EG. HANDLER CHANGED BELOW) UNSET IT YOU MUST USE ANDROID METHOD OR STILL OVERDRAWS, MAUI DOESN'T WORK TO NULL IT
            MainPage = mainPage;

            //=======================
            //THINGS TO SEE
            //=======================
            //See: https://developer.android.com/topic/performance/rendering/inspect-gpu-rendering
            //1) ContentPage naturally draws with background, setting to null and nothing else solves this.
            //2) If you set ContentPage background to any color (eg. black), then set it to null after using Maui API, this still overdraws using Maui (must use Android.Views.View.SetBackground(null) to reverse this). Not sure if this applies to other views too.
            //3) Another overdraw occurs due to DecorView being given a white background by default and needing this line to resolve: Platform.CurrentActivity.Window.DecorView.SetBackground(null);
            //Note also: Adding Border/BoxView elements increase overdraw even if they have no background, while Layout/Image elements do not (but this is normal due to the Box/Border being hardware layers)

            //=======================
            //FIXES
            //=======================
            //To get no overdraw:
            //1) ContentPage must be set to background null by default, and never set to anything else at any other time (or Android method must be used to null it once set otherwise)
            //2) If ContentPage background is set manually and must be unset, must use Android method SetBackground(null) - not sure if this applies to other view types.
            //3) Must run: Platform.CurrentActivity.Window.DecorView.SetBackground(null); 
            

            //===========================================
            //OPTIONALLY ADD MORE ITEMS TO SEE RESULT
            //===========================================
            //adding more layout objects overtop does not worsen the overdraw as long as they have no backgrounds
            List<VisualElement> veList = new();
            if (addMoreItems) {

                AbsoluteLayout abs = new();
                mainPage.Content = abs;

                for (int i = 0; i < numToAdd; i++) {
                    VisualElement ve = (VisualElement)Activator.CreateInstance(typeToAdd);
                    if (colorizeExtras) { ve.BackgroundColor = Colors.White; }
                    veList.Add(ve);
                    abs.Add(ve);
                }
            }
            if (animateOverElement) {
                if (addMoreItems) { 
                    animateElement(veList[veList.Count - 1]);
                }
                else {
                    animateElement(mainPage);
                }
            }

            //============================
            //SCREEN RESIZE FUNCTION
            //============================
            mainPage.SizeChanged += delegate {
                double width = mainPage.Width;
                double height = mainPage.Height;
                
                if (width > 0) {

                    for (int i=0; i < veList.Count; i++) {
                        veList[i].WidthRequest = width;
                        veList[i].HeightRequest = height;

                    }
                }
            };
            mainPage.HandlerChanged += delegate {
                mainPage.BackgroundColor = null; // DOES NOT WORK IF BACKGROUND COLOR WAS PREVIOUSLY SET IN ANDROID, STILL OVEDRAWS UNLESS YOU USE ANDROID METHOD BELOW
#if ANDROID

                //====================================================================
                //IF YOU COMMENT OUT EITHER OF THESE LINES YOU WILL GET OVERDRAW
                //====================================================================
                mainPage.ToPlatform(mainPage.Handler.MauiContext).SetBackground(null); //NECESSARY TO UNDO THE BAKCGROUND COLOR IF IT WAS SET BEFORE 
                Platform.CurrentActivity.Window.DecorView.SetBackground(null); //===============SOLVES THE EXTRA DRAW

#endif
            };
        }

        //==============================
        //IRRELEVANT TO OVERDRAW BUG
        //==============================
        public void animateElement(VisualElement element) {
            
                IDispatcherTimer timer = Application.Current.Dispatcher.CreateTimer();
                timer.Start();
                Color color1 = Colors.BlueViolet;
                //Color color1 = Colors.DeepSkyBlue;
                Color color2 = Colors.White;
                timer.Interval = TimeSpan.FromSeconds(1 / 120.0);
                double time = 0;
                double deltaTime = 0;
                DateTime lastDateTime = DateTime.Now;
                timer.Tick += delegate {
                    if (DateTime.Now != lastDateTime) {
                        deltaTime = (DateTime.Now - lastDateTime).TotalSeconds;
                        time += deltaTime;
                        double lerp = (Math.Sin(time) + 1) * 0.5;
                        Color colorToSet = Lerp(color1, color2, lerp);
                        element.BackgroundColor = colorToSet;
                        lastDateTime = DateTime.Now;
                    }
                };
        }
        public static Color Lerp(Color color1, Color color2, double pctToLerpFrom1To2) {
            float oneMinusLerp = 1 - (float)pctToLerpFrom1To2;
            float red = (float)(color1.Red * oneMinusLerp + color2.Red * pctToLerpFrom1To2);
            float green = (float)(color1.Green * oneMinusLerp + color2.Green * pctToLerpFrom1To2);
            float blue = (float)(color1.Blue * oneMinusLerp + color2.Blue * pctToLerpFrom1To2);
            float alpha = (float)(color1.Alpha * oneMinusLerp + color2.Alpha * pctToLerpFrom1To2);

            return new Color(red, green, blue, alpha);
        }
    }
}