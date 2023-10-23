namespace Overdraw_Bug {
    public partial class App : Application {
        public App() {
            InitializeComponent();

            //=====================
            //DEMO CONTROLS
            //=====================
            bool drawBackground = true;
            bool addMoreItems = false; //set false and can still see screen overdraws on nothing
            bool animateOverElement = true;
            Type typeToAdd = typeof(AbsoluteLayout); //change type to add here (Border/BoxView increase overdraw even when empty, Layout/Image do not)
            bool colorizeExtras = true; //add color to Bg or not
            int numToAdd = 3;

            //=====================
            //BUILD PAGE
            //=====================
            ContentPage mainPage = new();
            if (!drawBackground) {
                mainPage.BackgroundColor = null;
            }
            MainPage = mainPage;
            
            //=======================
            //ABNORMAL BEHAVIORS
            //=======================
            //See: https://developer.android.com/topic/performance/rendering/inspect-gpu-rendering
            //1) Empty project overdraws by 1 (blue screen with overdraw mode activated in Android phone) - set "addMoreItems = false" and build to see - should be white screen.
            //2) Adding Border/BoxView elements increase overdraw even if they have no background, while Layout/Image elements do not (things without backgrounds shouldn't cause overdraw)

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
        }
        public void animateElement(VisualElement element) {
            
                IDispatcherTimer timer = Application.Current.Dispatcher.CreateTimer();
                timer.Start();
                Color color1 = Colors.BlueViolet;
                //Color color1 = Colors.DeepSkyBlue;
                Color color2 = Colors.White;
                timer.Interval = TimeSpan.FromSeconds(1 / 60.0);
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