using System;
using System.Drawing;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;


namespace FaceCon.FaceCon
{
	public delegate void FaceChangedHandler(bool fasePresent);
	/// <summary>
	/// Класс, отвечающий за обнаружение лица на фотографии
	/// </summary>
	public class FaceDetector
	{
		private HaarCascade haarCascade;
		private Image<Gray, byte> image;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="facecon.FaceDetector"/> class.
		/// </summary>
		/// <param name='cascadePath'>
		/// Путь к файлу XML с настройками каскада Хаара
		/// </param>
		public FaceDetector (string cascadePath, Image<Gray, byte> imageToDetect)
		{
			image = imageToDetect;
			haarCascade = new HaarCascade(cascadePath);
		}
		
		public bool processImage(ref Image<Gray, byte> drawedFace)
		{
			var faces =  haarCascade.Detect(image,
				1.4, 4,
                HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                new Size(image.Width / 8, image.Height / 8),
			    new Size(image.Width, image.Height)
            );

			foreach (var face in faces) {
				drawedFace.Draw (face.rect, new Gray(double.MaxValue), 3);
			}
			
			return faces.Length > 0;
		}
	}
}

