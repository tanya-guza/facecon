using System;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace FaceCon.CommandService
{
	/// <summary>
	/// Класс для предварительной обработки изображений
	/// </summary>
	public class ImageProcessor
	{
		/// <summary>
		/// Исходное изображение
		/// </summary>
		private Emgu.CV.Image<Bgr,byte> sourceImage;
		/// <summary>
		/// Изображение с удаленным шумом
		/// </summary>
		private Emgu.CV.Image<Bgr,byte> reducedNoiseImage;
		/// <summary>
		/// Изображение в оттенках серого
		/// </summary>
		private Emgu.CV.Image<Gray,byte> grayscaleImage;
		/// <summary>
		/// Нормализованное изображение
		/// </summary>
		private Emgu.CV.Image<Gray,byte> normalizedImage;
		
		public ImageProcessor (Emgu.CV.Image<Bgr, byte> source)
		{
			sourceImage = source;
			reducedNoiseImage = new Emgu.CV.Image<Bgr, byte>(sourceImage.Size);
			grayscaleImage = new Emgu.CV.Image<Gray, byte>(sourceImage.Size);
			normalizedImage = new Emgu.CV.Image<Gray, byte>(sourceImage.Size);
			
			
			// Размываем изображение для устранения шума
			Emgu.CV.CvInvoke.cvSmooth(sourceImage.Ptr, reducedNoiseImage.Ptr,
				Emgu.CV.CvEnum.SMOOTH_TYPE.CV_MEDIAN, 3, 0, 0, 0);
			
			// Преобразуем изображение в оттенки серого
			Emgu.CV.CvInvoke.cvCvtColor(reducedNoiseImage.Ptr, grayscaleImage.Ptr, 
				Emgu.CV.CvEnum.COLOR_CONVERSION.CV_BGR2GRAY);
			
			// Нормализация изображения
			Emgu.CV.CvInvoke.cvNormalize(grayscaleImage.Ptr, normalizedImage.Ptr, 
			    0, 200, Emgu.CV.CvEnum.NORM_TYPE.CV_MINMAX, IntPtr.Zero);
		}
		
		/// <summary>
		/// Возвращает исходное изображение
		/// </summary>
		/// <value>
		/// The source image.
		/// </value>
		public Image<Bgr, byte> SourceImage
		{
			get
			{
				return this.sourceImage;
			}
		}
		
		/// <summary>
		/// Возвращает изображение с уменьшенными шумами
		/// </summary>
		/// <value>
		/// The reduced noice image.
		/// </value>
		public Image<Bgr, byte> ReducedNoiceImage
		{
			get
			{
				return this.reducedNoiseImage;
			}
		}
		
		/// <summary>
		/// Возвращает черно-белое изображение
		/// </summary>
		/// <value>
		/// The grayscale image.
		/// </value>
		public Image<Gray, byte> GrayscaleImage
		{
			get
			{
				return this.grayscaleImage;
			}
		}
		
		/// <summary>
		/// Возвращает нормализованное изображение
		/// </summary>
		/// <value>
		/// The normalized image.
		/// </value>
		public Image<Gray, byte> NormalizedImage
		{
			get
			{
				return this.normalizedImage;
			}
		}
	}
}

