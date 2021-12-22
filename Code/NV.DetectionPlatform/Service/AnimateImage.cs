using System;
using System.Drawing;
using System.Windows.Forms;


namespace NV.DetectionPlatform.Service { 
public class AnimateImage : Form
{
   static  string path = System.IO.Path.Combine(System.Windows.Forms.Application.StartupPath, "loading.gif");

    //Create a Bitmpap Object.
    Bitmap image = new Bitmap(path);
    bool currentlyAnimating = false;

   public AnimateImage()
    {
            InitializeComponent();
    }
    //This method begins the animation.
    public void animate()
    {
        if (!currentlyAnimating)
        {

            //Begin the animation only once.
            ImageAnimator.Animate(image, new EventHandler(this.OnFrameChanged));
            currentlyAnimating = true;
        }
    }

    private void OnFrameChanged(object o, EventArgs e)
    {

        //Force a call to the Paint event handler.
        this.Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {

        //Begin the animation.
        animate();

        //Get the next frame ready for rendering.
        ImageAnimator.UpdateFrames();

        //Draw the next frame in the animation.
        e.Graphics.DrawImage(this.image, new Point(0, 0));
    }

   private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // AnimateImage
            // 
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.ControlBox = false;
            this.Name = "AnimateImage";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.TransparencyKey = System.Drawing.Color.White;
            this.ResumeLayout(false);

        }
    }
}