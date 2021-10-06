// -----------------------------------------------------------------------
// <copyright file="MainPage.xaml.cs" company="John Lynch">
//   This file is licensed under the MIT license
//   Copyright (c) 2021 John Lynch
// </copyright>
// -----------------------------------------------------------------------



// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MinimalInk {
	using System.Collections.Generic;
	using System.Reflection.Metadata;
	using System.Threading.Tasks;

	using Windows.UI;
	using Windows.UI.Input.Inking;

	using pdftron;
	using pdftron.Common;
	using pdftron.PDF;
	using pdftron.PDF.Annots;

	using Page = Windows.UI.Xaml.Controls.Page;

	/// <summary>
	///     An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page {
		private readonly PDFViewCtrl PDFViewCtrl;

		private readonly InkSynchronizer Synchronizer;

		public MainPage() {
			this.InitializeComponent();

			this.InkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(
				new InkDrawingAttributes { IgnorePressure = true, Size = new Windows.Foundation.Size(3,3), Color = Colors.Red });
			this.Synchronizer = this.InkCanvas.InkPresenter.ActivateCustomDrying();
			this.InkCanvas.InkPresenter.StrokesCollected += this.InkPresenter_StrokesCollected;

			PDFNet.Initialize();
			this.PDFViewCtrl = new PDFViewCtrl();

			PDFDoc Doc = new PDFDoc();
			Doc.PagePushBack(Doc.PageCreate());
			this.PDFViewCtrl.SetDoc(Doc);
			this.PDFViewCtrl.SetZoom(2);
			this.PDFViewCtrl.SetZoomEnabled(false);

			this.PDFViewBorder.Child = this.PDFViewCtrl;
		}

		private async void InkPresenter_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args) {
			IReadOnlyList<InkStroke> Strokes = this.Synchronizer.BeginDry();

			PDFDoc PdfDoc = this.PDFViewCtrl.GetDoc();
			Ink InkAnnotation = Ink.Create(PdfDoc.GetSDFDoc(), new Rect(0,0,1000,1000));
			IReadOnlyList<InkPoint> List = Strokes[0].GetInkPoints();
			for (int i = 0; i < List.Count; i++) {
				InkPoint Point = List[i];

				DoubleRef XRef = new DoubleRef(Point.Position.X);
				DoubleRef YRef = new DoubleRef(Point.Position.Y);
				this.PDFViewCtrl.ConvScreenPtToPagePt(XRef, YRef, 1);
				InkAnnotation.SetPoint(0, i, new Point(XRef.Value, YRef.Value));
			}

			PdfDoc.GetPage(1).AnnotPushBack(InkAnnotation);
			this.PDFViewCtrl.UpdateWithAnnot(InkAnnotation, 1);

			await Task.Delay(100);
			this.Synchronizer.EndDry();
		}
	}
}