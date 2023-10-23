namespace Overdraw_Bug {
    public partial class App : Application {
        public App() {
            InitializeComponent();

            ContentPage mainPage = new();
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
            bool addMoreItems = true; //set false and can still see screen overdraws on nothing
            Type typeToAdd = typeof(AbsoluteLayout); //change type to add here (Border/BoxView increase overdraw even when empty, Layout/Image do not)
            bool colorizeBg = false; //add color to Bg or not
            int numToAdd = 3;

            //adding more layout objects overtop does not worsen the overdraw as long as they have no backgrounds
            List<VisualElement> veList = new();
            if (addMoreItems) {

                AbsoluteLayout abs = new();
                mainPage.Content = abs;

                for (int i = 0; i < numToAdd; i++) {
                    VisualElement ve = (VisualElement)Activator.CreateInstance(typeToAdd);
                    if (colorizeBg) { ve.BackgroundColor = Colors.White; }
                    veList.Add(ve);
                    abs.Add(ve);
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
    }
}