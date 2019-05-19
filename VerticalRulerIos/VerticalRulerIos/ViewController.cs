using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using UIKit;

namespace VerticalRulerIos
{
    public class OffsetPriceModel
    {
        public nfloat Offset { get; set; }

        public int Price { get; set; }
    }

    public partial class ViewController : UIViewController
    {
        List<OffsetPriceModel> offsetPriceList = new List<OffsetPriceModel>();
        UITextField priceTF;
        nfloat HalfScrollHeight;
        bool IsScrolling { get; set; }
        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            InitElements();
            ScrollingSubs();
        }

        private void InitElements()
        {
            var halfScreen = View.Frame.Width / 2;
            scrollView.BackgroundColor = UIColor.Cyan;
            scrollView.Frame = new CGRect(halfScreen + halfScreen / 2, View.Frame.Height / 10, halfScreen / 2, View.Frame.Height - View.Frame.Height / 5);
            scrollView.ShowsVerticalScrollIndicator = false;
            var longLineMargin = scrollView.Frame.Width / 10;
            nfloat longLineWidth = scrollView.Frame.Width - longLineMargin;
            nfloat shortLineWidth = scrollView.Frame.Width / 8;
            nfloat lineWidth = 0;
            HalfScrollHeight = scrollView.Frame.Height / 2;
            var lineHeight = 1;
            var verticalStep = 15;
            var verticalStep2x = verticalStep * 2;
            var moneyCount = 3001; // 100 dollars means 100 dividers

            int scrollCoefficient = 120; // Be careful with coefficient. Check on different screen resolutions scrolling to max value. We need coefficient besause we go from the bottom

            priceTF = new UITextField();
            priceTF.TextAlignment = UITextAlignment.Center;
            priceTF.Font = UIFont.FromName("Helvetica-Bold", 23F);
            priceTF.TextColor = UIColor.Red;
            priceTF.Frame = new CGRect(0, 0, halfScreen, View.Frame.Height);
            // TODO number pad with return key and keyboard shouldn`t overlay price on SE
            priceTF.KeyboardType = UIKeyboardType.NumberPad;
            priceTF.EditingChanged += PriceTF_EditingChanged;

            View.AddSubview(priceTF);

            for (int i = 0; i < moneyCount; i++)
            {
                if (i % 10 == 0)
                {
                    lineWidth = longLineWidth;
                    var subPriceLabel = new UILabel();
                    subPriceLabel.TextColor = UIColor.Red;
                    subPriceLabel.Text = i.ToString();
                    subPriceLabel.Frame = new CGRect(longLineMargin + 10, scrollView.Frame.Height * scrollCoefficient - verticalStep * i - lineHeight * i - HalfScrollHeight - verticalStep2x, lineWidth, lineHeight * scrollCoefficient);
                    scrollView.AddSubview(subPriceLabel);
                }
                else
                    lineWidth = shortLineWidth;
                var stepView = new UIView();
                stepView.BackgroundColor = UIColor.Gray;
                stepView.Frame = new CGRect(scrollView.Frame.Width - lineWidth, scrollView.Frame.Height * scrollCoefficient - verticalStep * i - lineHeight * i - HalfScrollHeight, lineWidth, lineHeight);
                scrollView.AddSubview(stepView);
                offsetPriceList.Add(new OffsetPriceModel { Offset = stepView.Frame.Y, Price = i });
            }
            scrollView.ContentSize = new CGSize(scrollView.Frame.Width, /*lastOffset);*/scrollView.Frame.Height * scrollCoefficient);
            scrollView.ContentOffset = new CGPoint(0, scrollView.ContentSize.Height - scrollView.Frame.Height);

            var blueView = new UIView();
            blueView.BackgroundColor = UIColor.Blue;
            blueView.UserInteractionEnabled = false;
            blueView.Frame = new CGRect(scrollView.Frame.X, scrollView.Frame.Y + scrollView.Frame.Height / 2, scrollView.Frame.Width, lineHeight * 3);
            View.AddSubview(blueView);
        }

        nfloat CalculateClosest(nfloat visibleCenter)
        {
            var closestGreater = offsetPriceList.Last(p => p.Offset >= visibleCenter).Offset;
            var closestLesser = offsetPriceList.First(p => p.Offset <= visibleCenter).Offset;

            if (Math.Abs(visibleCenter - closestGreater) < Math.Abs(visibleCenter - closestLesser))
            {
                var index = offsetPriceList.FindIndex(x => x.Offset == closestGreater);
                var price = offsetPriceList[index].Price;
                priceTF.Text = price.ToString();
                return closestGreater;
            }
            else
            {
                var index = offsetPriceList.FindIndex(x => x.Offset == closestLesser);
                var price = offsetPriceList[index].Price;
                priceTF.Text = price.ToString();
                return closestLesser;
            }
        }

        private void ScrollingSubs()
        {
            scrollView.DraggingEnded += (s, e) => { try { StickOffset(); } catch { } };
            scrollView.DecelerationEnded += (s, e) => { try { StickOffset(); } catch { } };
        }

        void StickOffset()
        {
            var center = scrollView.ContentOffset.Y + HalfScrollHeight;
            var closestDivider = CalculateClosest(center);
            var offset = new CGPoint(0, closestDivider - HalfScrollHeight);
            scrollView.SetContentOffset(offset, true);
        }

        void PriceTF_EditingChanged(object sender, EventArgs e)
        {
            try
            {
                var item = offsetPriceList.Find(p => p.Price == Convert.ToInt32(priceTF.Text));
                var offset = new CGPoint(0, item.Offset - HalfScrollHeight);
                scrollView.SetContentOffset(offset, true);
            }
            catch
            {
                // Hide scrollView for manual entering
            }
        }
    }
}