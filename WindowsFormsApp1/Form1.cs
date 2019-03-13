using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    
    public partial class Form1 : Form
    {
        int meter = 60;
        int bound = 10;
        IList<detectedItem> ItemList;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            Bitmap bmp;
            if (file.ShowDialog() == DialogResult.OK) {
                bmp = new Bitmap(file.FileName);
                pictureBox1.Image = bmp;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ItemList = new List<detectedItem>();
            Bitmap bmp = new Bitmap(pictureBox1.Image);
            Bitmap temp = new Bitmap(pictureBox1.Image);
            Graphics g = pictureBox1.CreateGraphics();
            /*
            
            */
            bool flag = true;
            //For detecting the item
            //and get the left most point
            int i = 0;
            while (flag)
            {
                detectedItem x = itemDetector(temp);
                
                if(x != null)
                {
                    ItemList.Add(x);
                    //ItemList[0].toString();

                    temp = this.eraser(ItemList[i].x[0], ItemList[i].y[1], ItemList[i].x[2] - ItemList[i].x[0], ItemList[i].y[3] - ItemList[i].y[1], bmp);
                }
                else
                {
                    flag = false;
                }

                i++;
            }


            Pen p = new Pen(Color.Red);
            Console.WriteLine("Lenght:\t" + ItemList.Count);
            for (int j = 0; j < ItemList.Count; j++)
                g.DrawRectangle(p, ItemList[j].x[0], ItemList[j].y[1], ItemList[j].x[2] - ItemList[j].x[0], ItemList[j].y[3] - ItemList[j].y[1]);
            
            //pictureBox1.Image = temp;
            //temp.Save("C:\\Users\\musta\\Desktop\\result.png", System.Drawing.Imaging.ImageFormat.Png);
        }

        private detectedItem itemDetector(Bitmap temp) {
            bool flag = false;
            detectedItem item = null;
            for (int w = 0; w < temp.Width; w++)
            {
                for (int h = 0; h < temp.Height; h++)
                {
                    if (temp.GetPixel(w, h) == Color.Black || temp.GetPixel(w, h).R < meter)
                    {
                        //Console.WriteLine("W:"+w+"\tH:"+h);
                        item  = this.pathFinder(w, h, temp);
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    break;
                }
            }

            return item;
        }
        private Bitmap eraser(int x,int y,int width,int height, Bitmap bmp) {

            for (int w = x; w < width+x && w < bmp.Width; w++){
                for (int h = y; h < height+y && h < bmp.Height; h++) {
                    bmp.SetPixel(w, h, Color.White);
                }
            }

            //Console.WriteLine("\nWidth:" + width + "\tHeight:" + height);
            //Console.WriteLine("\nx:" + x + "\ty:" + y);
            return bmp;
        } 

        private detectedItem pathFinder(int w, int h, Bitmap bmp) {
            detectedItem item = new detectedItem();

            //we have the coordinated of left most pixel
            item.x[0] = w;
            item.y[0] = h;


            // the upper and lower bound can be improved time required to solve the math
            int top, bottom;
            if (h - bound < 0){
                top = h;
            }
            else{
                top = h - bound;
            }

            if (h + bound > bmp.Height){
                bottom = h;

            }
            else{
                bottom = h + bound;
            }
            

            bool flag = false;

            //for right most point
            for (int width = w; ; width++) {
                for (int height = top; height <= bottom; height++) {
                    if (bmp.GetPixel(width, height) == Color.Black || bmp.GetPixel(width, height).R < meter) {
                        break;
                    }

                    if(height == bottom){
                        flag = true;
                    }
                }
                if (flag){
                   for(int t = top; t <= bottom; t++) {
                        if(bmp.GetPixel(width-1,t) == Color.Black || bmp.GetPixel(width-1,t).R < meter){
                            item.x[2] = width;
                            item.y[2] = t;
                            break;
                        }
                    }
                    break;
                }
            }


            int left = item.x[0], right = item.x[2];
            flag = false;
            
            //for top most
            for (int height = item.y[0]; ; height--) {
                for (int width = item.x[0]; width <= item.x[2]; width++) {
                    if (bmp.GetPixel(width, height) == Color.Black || bmp.GetPixel(width, height).R < meter) {
                        break;
                    }
                    
                    if (width == item.x[2]){
                        flag = true;
                    }
                }
                if (flag){
                    for (int t = left; t <= right; t++){
                        if (bmp.GetPixel(t, height+1) == Color.Black || bmp.GetPixel(t, height+1).R < meter){
                            item.x[1] = t;
                            item.y[1] = height;
                            break;
                        }
                    }
                    break;
                }
            }

            
            flag = false;
            //for bottom most
            for (int height = item.y[0]; ; height++)
            {
                for (int width = item.x[0]; width <= item.x[2]; width++)
                {
                    if (bmp.GetPixel(width, height) == Color.Black || bmp.GetPixel(width, height).R < meter)
                    {
                        break;
                    }

                    if (width == item.x[2])
                    {
                        flag = true;
                    }
                }
                if (flag)
                {
                    for (int t = left; t <= right; t++)
                    {
                        if (bmp.GetPixel(t, height - 1) == Color.Black || bmp.GetPixel(t, height - 1).R < meter)
                        {
                            item.x[3] = t;
                            item.y[3] = height;
                            break;
                        }
                    }
                    break;
                }
            }

            item = errorHandeler(item,bmp);

            return item;
        }

        private detectedItem errorHandeler(detectedItem item, Bitmap bmp)
        {
            int topLeftx = item.x[0];
            int topLefty = item.y[1];
            int height = item.y[3] - item.y[1];
            int width = item.x[2] - item.x[0];

            bool flag = true;
            //top side error
            while (flag)
            {
                if (topError(item.x[0], item.y[1] + 1, (item.x[2] - item.x[0]), bmp))
                {
                    //Console.WriteLine("top");
                    item.y[1] -= 1;
                    for (int x = item.x[0]; x < (item.x[2] - item.x[0]) + item.x[0]; x++)
                    {
                        if (bmp.GetPixel(x, item.y[1]) == Color.Black || bmp.GetPixel(x, item.y[1]).R < meter)
                        {
                            item.x[1] = x;
                        }
                    }

                } else if (rightError(item.x[2] - 1, item.y[1], (item.y[3] - item.y[1]), bmp))
                {
                    // Console.WriteLine("right");
                    item.x[2] += 1;
                    for (int y = item.y[1]; y < (item.y[3] - item.y[1]) + item.y[1]; y++)
                    {
                        if (bmp.GetPixel(item.x[2], y) == Color.Black || bmp.GetPixel(item.x[2], y).R < meter)
                        {
                            item.y[2] = y;
                        }
                    }

                }
                else if (bottomError(item.x[0], item.y[3] - 1, item.x[2] - item.x[0], bmp))
                {
                    //Console.WriteLine("bottom");
                    item.y[3] += 1;
                    for (int x = item.x[0]; x < (item.x[2] - item.x[0]) + item.x[0]; x++)
                    {
                        if (bmp.GetPixel(x, item.y[3]) == Color.Black || bmp.GetPixel(x, item.y[3]).R < meter)
                        {
                            item.x[1] = x;
                        }
                    }
                } 
                else
                {
                    flag = false;
                }
            }


            return item;
        }

        private bool topError(int x, int y, int width, Bitmap bmp){
            //Console.WriteLine("X:" + x + "\tY:" + y+"\tWidth:"+width);
            for(int i = x; i < x+width; i++)
            {
                
                if (bmp.GetPixel(i,y-1) == Color.Black || bmp.GetPixel(i,y-1).R < meter)
                {
                    //Console.WriteLine("error detected");
                    return true;
                }
            }
            return false;
        }

        private bool rightError(int x, int y, int height, Bitmap bmp)
        {
            //Console.WriteLine("\nX:"+x+"\tY:"+y+"\tHeight:"+height);
            for(int i = y; i < y+height; i++)
            {
                //Console.WriteLine("inside");
                if (bmp.GetPixel(x+1,i) == Color.Black || bmp.GetPixel(x+1,i).R < meter)
                {
                    return true;
                }
            }
            return false;
        }

        

        private bool bottomError(int x, int y, int width, Bitmap bmp)
        {
            for(int i = x; i <= width+x; i++)
            {
                if (bmp.GetPixel(i, y + 1) == Color.Black || bmp.GetPixel(i, y + 1).R < meter) {
                    return true;
                }
            }
            return false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Bitmap temp = new Bitmap(pictureBox1.Image);
            for (int w = 0; w < temp.Width; w++)
            {
                for (int h = 0; h < temp.Height; h++)
                {
                    int total;
                    int red = temp.GetPixel(w, h).R;
                    int green = temp.GetPixel(w, h).G;
                    int blue = temp.GetPixel(w, h).B;

                    total = (red + green + blue) / 3;

                    temp.SetPixel(w, h, Color.FromArgb(total, total, total));
                }
            }

            for (int w = 0; w < temp.Width; w++)
            {
                for (int h = 0; h < temp.Height; h++)
                {
                    if (temp.GetPixel(w, h) == Color.Black || temp.GetPixel(w, h).R < 150)
                    {
                        temp.SetPixel(w, h, Color.Black);
                    }
                    else
                    {
                        temp.SetPixel(w, h, Color.White);
                    }
                }
            }
            //temp.Save("C:\\Users\\musta\\Desktop\\result1.png", System.Drawing.Imaging.ImageFormat.Png);
            pictureBox1.Image = temp;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }

    class detectedItem
    {
        public int[] x = new int[4];
        public int[] y = new int[4];
        public detectedItem()
        {

        }

        public void toString()
        {
            for (int i = 0; i < 4; i++)
            {
                Console.WriteLine("\nx" + i + ":" + x[i] + "\ty" + i + ":" + y[i]);
            }
        }
    }
}
