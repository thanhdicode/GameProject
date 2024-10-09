using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SpaceShooterGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer gameTimer = new DispatcherTimer();
        private DispatcherTimer obstacleTimer = new DispatcherTimer();
        private DispatcherTimer bossTimer = new DispatcherTimer();
        private DispatcherTimer bossFireTimer = new DispatcherTimer();
        private DispatcherTimer bulletTimer = new DispatcherTimer();
        private DispatcherTimer buffTimer = new DispatcherTimer();
        private Random rand = new Random();
        private bool moveLeft, moveRight;
        private int playerSpeed = 10;
        private int score = 0;
        private int playerHealth = 3;
        private bool bossAppeared = false;
        private bool canShoot = true;
        private int bossScoreMultiplier = 5;
        private int bossHealth = 300;
        private const double BossBulletSpeed = 5;
        private const int NumberOfBossBullets = 8;
        private double bossBulletAngleIncrement;
        private int currentBossBulletIndex = 0;
        private double bossBulletBaseAngle = 0;
        private TextBlock bossMessage;
        private int maxHealth = 3; // Maximum health
        private int bossShotsFired = 0; // To keep track of the number of shots fired by the boss
        private DispatcherTimer bossFireDelayTimer = new DispatcherTimer(); // Timer for delay after two shots
        private int ammoBuffCount = 0; // Biến để theo dõi số lần nhận buff ammo

        public MainWindow()
        {
            InitializeComponent();

            gameTimer.Interval = TimeSpan.FromMilliseconds(20);
            gameTimer.Tick += GameLoop;
            gameTimer.Start();

            obstacleTimer.Interval = TimeSpan.FromSeconds(2);
            obstacleTimer.Tick += CreateObstacle;
            obstacleTimer.Start();

            bossTimer.Interval = TimeSpan.FromSeconds(30);
            bossTimer.Tick += (s, e) => PrepareBossAppearance();
            bossTimer.Start();

            bulletTimer.Interval = TimeSpan.FromMilliseconds(300);
            bulletTimer.Tick += (s, e) => canShoot = true;
            bulletTimer.Start();
            buffTimer.Interval = TimeSpan.FromSeconds(10); // Adjust interval as needed
            buffTimer.Tick += CreateBuff;
            buffTimer.Start();
            UpdateScoreAndHealth();
        }

        private void GameLoop(object sender, EventArgs e)
        {
            var itemsToRemove = new List<UIElement>();

            // Handle player movement
            if (moveLeft && Canvas.GetLeft(PlayerShip) > 0)
            {
                Canvas.SetLeft(PlayerShip, Canvas.GetLeft(PlayerShip) - playerSpeed);
            }
            if (moveRight && Canvas.GetLeft(PlayerShip) + PlayerShip.Width < GameCanvas.ActualWidth)
            {
                Canvas.SetLeft(PlayerShip, Canvas.GetLeft(PlayerShip) + playerSpeed);
            }

            foreach (var item in GameCanvas.Children.OfType<Image>())
            {
                var tag = item.Tag as dynamic;
                if (tag != null)
                {
                    // Handle obstacles
                    if (tag.Type == "obstacle")
                    {
                        Canvas.SetTop(item, Canvas.GetTop(item) + 5);
                        if (Canvas.GetTop(item) > GameCanvas.ActualHeight)
                        {
                            itemsToRemove.Add(item);
                        }
                        else if (IsColliding(PlayerShip, item))
                        {
                            itemsToRemove.Add(item);
                            playerHealth--;
                            UpdateScoreAndHealth();
                            if (playerHealth <= 0)
                            {
                                EndGame();
                                return;
                            }
                        }
                    }

                    // Handle boss
                    if (tag.Type == "boss")
                    {
                        Canvas.SetTop(item, 30);

                        if (IsColliding(PlayerShip, item))
                        {
                            playerHealth--;
                            UpdateScoreAndHealth();
                            if (playerHealth <= 0)
                            {
                                EndGame();
                                return;
                            }
                        }
                    }

                    // Handle boss bullets
                    if (tag.Type == "bossBullet")
                    {
                        double xSpeed = tag.XSpeed;
                        double ySpeed = tag.YSpeed;
                        Canvas.SetLeft(item, Canvas.GetLeft(item) + xSpeed);
                        Canvas.SetTop(item, Canvas.GetTop(item) + ySpeed);
                        if (Canvas.GetTop(item) > GameCanvas.ActualHeight)
                        {
                            itemsToRemove.Add(item);
                        }
                        else if (IsColliding(PlayerShip, item))
                        {
                            itemsToRemove.Add(item);
                            playerHealth--;
                            UpdateScoreAndHealth();
                            if (playerHealth <= 0)
                            {
                                EndGame();
                                return;
                            }
                        }
                    }

                    // Handle player bullets
                    if (tag.Type == "bullet")
                    {
                        Canvas.SetTop(item, Canvas.GetTop(item) - 10);
                        if (Canvas.GetTop(item) < 0)
                        {
                            itemsToRemove.Add(item);
                        }

                        var hitTarget = GameCanvas.Children.OfType<Image>().FirstOrDefault(r =>
                            r != item && IsColliding(item, r) && ((r.Tag as dynamic)?.Type == "obstacle" || (r.Tag as dynamic)?.Type == "boss"));

                        if (hitTarget != null)
                        {
                            itemsToRemove.Add(item);

                            var hitHealth = (hitTarget.Tag as dynamic)?.Health ?? 0;
                            var bulletDamage = (item.Tag as dynamic)?.Damage ?? 0;

                            hitHealth -= bulletDamage;

                            if (hitHealth <= 0)
                            {
                                itemsToRemove.Add(hitTarget);
                                if ((hitTarget.Tag as dynamic)?.Type == "boss")
                                {
                                    bossAppeared = false;
                                    bossTimer.Start();
                                    score += bossScoreMultiplier * 100;
                                    bossScoreMultiplier++;

                                    // Remove boss message
                                    if (bossMessage != null)
                                    {
                                        GameCanvas.Children.Remove(bossMessage);
                                        bossMessage = null;
                                    }
                                }
                                else
                                {
                                    score += 100;
                                }
                            }
                            else
                            {
                                hitTarget.Tag = new { Type = (hitTarget.Tag as dynamic)?.Type, Health = hitHealth };
                            }

                            UpdateScoreAndHealth();
                        }
                    }
                    if (tag.Type == "ammoBuff" || tag.Type == "healthBuff" || tag.Type == "damageBuff")
                    {
                        Canvas.SetTop(item, Canvas.GetTop(item) + 5);
                        if (Canvas.GetTop(item) > GameCanvas.ActualHeight)
                        {
                            itemsToRemove.Add(item);
                        }
                        else if (IsColliding(PlayerShip, item))
                        {
                            itemsToRemove.Add(item);
                            ApplyBuff(tag.Type);
                        }
                    }
                }
            }

            if (bossAppeared)
            {
                foreach (var item in GameCanvas.Children.OfType<Image>().Where(r => (r.Tag as dynamic)?.Type == "obstacle"))
                {
                    itemsToRemove.Add(item);
                }
            }

            foreach (var item in itemsToRemove)
            {
                GameCanvas.Children.Remove(item);
            }

            FireBullet();
        }


        private void ApplyBuff(string buffType)
        {
            switch (buffType)
            {
                case "ammoBuff":
                    ammoBuffCount++;
                    switch (ammoBuffCount)
                    {
                        case 1:
                            // Thay đổi hình ảnh đạn
                            bulletTimer.Interval = TimeSpan.FromMilliseconds(300); // Thay đổi khoảng thời gian giữa các lần bắn
                            foreach (var bullet in GameCanvas.Children.OfType<Image>().Where(r => (r.Tag as dynamic)?.Type == "bullet"))
                            {
                                bullet.Source = new BitmapImage(new Uri("pack://application:,,,/assesst_shootergame/buff_ammo.png"));
                            }
                            break;
                        case 2:
                            // Bắn ra 3 tia
                            FireTripleBullets();
                            break;
                        case 3:
                            // Thay đổi hình ảnh đạn và bắn ra 3 tia
                            foreach (var bullet in GameCanvas.Children.OfType<Image>().Where(r => (r.Tag as dynamic)?.Type == "bullet"))
                            {
                                bullet.Source = new BitmapImage(new Uri("pack://application:,,,/assesst_shootergame/buff_ammo.png"));
                            }
                            break;
                        case 4:
                            // Bắn đạn nhanh hơn
                            bulletTimer.Interval = TimeSpan.FromMilliseconds(100); // Bắn nhanh hơn
                            break;
                    }
                    break;
                case "healthBuff":
                    // Tăng sức khỏe
                    if (playerHealth < maxHealth)
                    {
                        playerHealth++;
                        UpdateScoreAndHealth();
                    }
                    break;
                case "damageBuff":
                    // Tăng sát thương đạn
                    foreach (var bullet in GameCanvas.Children.OfType<Image>().Where(r => (r.Tag as dynamic)?.Type == "bullet"))
                    {
                        var tag = (dynamic)bullet.Tag;
                        var newTag = new { Type = tag.Type, Damage = tag.Damage + 1 }; // Tăng sát thương
                        bullet.Tag = newTag;
                    }
                    break;
            }
        }
        private void UpdateBullets()
        {
            foreach (var bullet in GameCanvas.Children.OfType<Image>().Where(r => (r.Tag as dynamic)?.Type == "bullet"))
            {
                var tag = (dynamic)bullet.Tag;
                Canvas.SetTop(bullet, Canvas.GetTop(bullet) + tag.YSpeed); // Di chuyển theo Y
                Canvas.SetLeft(bullet, Canvas.GetLeft(bullet) + tag.XSpeed); // Di chuyển theo X

                // Kiểm tra va chạm và loại bỏ đạn nếu cần
                if (Canvas.GetTop(bullet) < 0)
                {
                    GameCanvas.Children.Remove(bullet);
                }
            }
        }

        private bool IsColliding(Image img1, Image img2)
        {
            Rect hitBox1 = new Rect(Canvas.GetLeft(img1), Canvas.GetTop(img1), img1.Width, img1.Height);
            Rect hitBox2 = new Rect(Canvas.GetLeft(img2), Canvas.GetTop(img2), img2.Width, img2.Height);
            return hitBox1.IntersectsWith(hitBox2);
        }
        private void FireTripleBullets()
        {
            if (canShoot)
            {
                for (int i = -1; i <= 1; i++)
                {
                    Image newBullet = new Image
                    {
                        Source = new BitmapImage(new Uri("pack://application:,,,/assesst_shootergame/buff_ammo.png")),
                        Width = 40,
                        Height = 40,
                        Tag = new { Type = "bullet", Damage = 8 }
                    };

                    Canvas.SetLeft(newBullet, Canvas.GetLeft(PlayerShip) + PlayerShip.Width / 2 - newBullet.Width / 2);
                    Canvas.SetTop(newBullet, Canvas.GetTop(PlayerShip) - newBullet.Height);

                    // Tính toán góc bắn
                    double xSpeed = 0;
                    double ySpeed = -10;
                    if (i != 0)
                    {
                        xSpeed = i * 5; // Điều chỉnh khoảng cách giữa các tia
                    }

                    newBullet.Tag = new { Type = "bullet", Damage = 8, XSpeed = xSpeed, YSpeed = ySpeed };

                    GameCanvas.Children.Add(newBullet);
                }

                canShoot = false;
            }
        }


        private void FireBullet()
        {
            if (canShoot)
            {
                int numberOfBullets = 1; // Số lượng đạn bắn ra
                if (ammoBuffCount == 1)
                {
                    Image newBullet = new Image
                    {
                        Source = new BitmapImage(new Uri("pack://application:,,,/assesst_shootergame/buff_ammo.png")),
                        Width = 40,
                        Height = 40,
                        Tag = new { Type = "bullet", Damage = 8 }
                    };

                    Canvas.SetLeft(newBullet, Canvas.GetLeft(PlayerShip) + PlayerShip.Width / 2 - newBullet.Width / 2);
                    Canvas.SetTop(newBullet, Canvas.GetTop(PlayerShip) - newBullet.Height);

                    GameCanvas.Children.Add(newBullet);
                }

                if (ammoBuffCount >= 2)
                {
                    // Bắn ra 3 tia nếu nhận buff lần thứ hai
                    FireTripleBullets();
                }
                else
                {
                    // Bắn một viên đạn bình thường
                    Image newBullet = new Image
                    {
                        Source = new BitmapImage(new Uri("pack://application:,,,/assesst_shootergame/bullet.png")),
                        Width = 40,
                        Height = 40,
                        Tag = new { Type = "bullet", Damage = 8 }
                    };

                    Canvas.SetLeft(newBullet, Canvas.GetLeft(PlayerShip) + PlayerShip.Width / 2 - newBullet.Width / 2);
                    Canvas.SetTop(newBullet, Canvas.GetTop(PlayerShip) - newBullet.Height);

                    GameCanvas.Children.Add(newBullet);
                }

                canShoot = false;
            }
        }


        private void CreateObstacle(object sender, EventArgs e)
        {
            if (bossAppeared) return;

            Image newObstacle = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/assesst_shootergame/obstancle.png")),
                Width = 50,
                Height = 50,
                Tag = new { Type = "obstacle", Health = rand.Next(10, 16) }
            };
            Canvas.SetLeft(newObstacle, rand.Next(0, (int)GameCanvas.ActualWidth - 50));
            Canvas.SetTop(newObstacle, -50);
            GameCanvas.Children.Add(newObstacle);
        }

        private void PrepareBossAppearance()
        {
            if (!bossAppeared)
            {
                // Remove existing obstacles
                var itemsToRemove = GameCanvas.Children.OfType<Image>().Where(r => (r.Tag as dynamic)?.Type == "obstacle").ToList();
                foreach (var item in itemsToRemove)
                {
                    GameCanvas.Children.Remove(item);
                }

                // Display boss message
                bossMessage = new TextBlock
                {
                    Text = "BOSS!!!",
                    FontSize = 40,
                    Foreground = Brushes.Red,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Canvas.SetLeft(bossMessage, (GameCanvas.ActualWidth - bossMessage.ActualWidth) / 2);
                Canvas.SetTop(bossMessage, (GameCanvas.ActualHeight - bossMessage.ActualHeight) / 2);
                GameCanvas.Children.Add(bossMessage);

                // Create the boss
                Image boss = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/assesst_shootergame/boss.png")),
                    Width = 100,
                    Height = 100,
                    Tag = new { Type = "boss", Health = bossHealth }
                };
                Canvas.SetLeft(boss, (GameCanvas.ActualWidth - boss.Width) / 2);
                Canvas.SetTop(boss, 30);
                GameCanvas.Children.Add(boss);
                if (bossMessage != null)
                {
                    GameCanvas.Children.Remove(bossMessage);
                    bossMessage = null;
                }
                // Set up boss shooting
                bossFireTimer.Interval = TimeSpan.FromSeconds(0.5); // Reduced interval for faster shooting
                bossFireTimer.Tick += (s, e) =>
                {
                    if (bossAppeared)
                    {
                        FireBossBullets(boss);
                    }
                };
                bossFireTimer.Start();

                bossAppeared = true;
                bossTimer.Stop();
                bossBulletAngleIncrement = 360.0 / NumberOfBossBullets; // Calculate angle increment
            }
        }


        private void FireBossBullets(Image boss)
        {
            int numberOfBullets = NumberOfBossBullets;

            for (int i = 0; i < numberOfBullets; i++)
            {
                // Calculate the angle for each bullet
                double angle = bossBulletBaseAngle + i * (360.0 / numberOfBullets); // Evenly spaced angles
                double radians = angle * (Math.PI / 180);
                double xSpeed = BossBulletSpeed * Math.Cos(radians);
                double ySpeed = BossBulletSpeed * Math.Sin(radians);

                Image bossBullet = new Image
                {
                    Source = new BitmapImage(new Uri("pack://application:,,,/assesst_shootergame/bullet.png")),
                    Width = 30,
                    Height = 20,
                    Tag = new { Type = "bossBullet", XSpeed = xSpeed, YSpeed = ySpeed }
                };

                Canvas.SetLeft(bossBullet, Canvas.GetLeft(boss) + boss.Width / 2 - bossBullet.Width / 2);
                Canvas.SetTop(bossBullet, Canvas.GetTop(boss) + boss.Height);

                GameCanvas.Children.Add(bossBullet);
            }

            // Update base angle for next fire to create a rotating effect
            bossBulletBaseAngle += 5; // Adjust this value to change the speed of rotation
            if (bossBulletBaseAngle >= 360)
            {
                bossBulletBaseAngle -= 360;
            }
            bossShotsFired++;

            if (bossShotsFired % 2 == 0)
            {
                bossFireTimer.Stop();
                bossFireDelayTimer.Interval = TimeSpan.FromSeconds(1.5);
                bossFireDelayTimer.Tick += (s, e) =>
                {
                    bossFireDelayTimer.Stop();
                    bossFireTimer.Start();
                };
                bossFireDelayTimer.Start();
            }
        }


        private void UpdateScoreAndHealth()
        {
            ScoreTextBlock.Text = $"Score: {score}";
            HealthTextBlock.Text = $"Health: {playerHealth}";
        }

        private void EndGame()
        {
            gameTimer.Stop();
            obstacleTimer.Stop();
            bossTimer.Stop();
            bossFireTimer.Stop();
            bulletTimer.Stop();

            MessageBox.Show($"Game Over! Final Score: {score}", "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);
            Application.Current.Shutdown();
        }
        private void CreateBuff(object sender, EventArgs e)
        {
            Image newBuff = new Image();
            int buffType = rand.Next(1, 4); // Randomly select a buff type

            switch (buffType)
            {
                case 1:
                    newBuff.Source = new BitmapImage(new Uri("pack://application:,,,/assesst_shootergame/buff_ammo.png"));
                    newBuff.Tag = new { Type = "ammoBuff" };
                    break;
                case 2:
                    newBuff.Source = new BitmapImage(new Uri("pack://application:,,,/assesst_shootergame/buff_health.png"));
                    newBuff.Tag = new { Type = "healthBuff" };
                    break;
                case 3:
                    newBuff.Source = new BitmapImage(new Uri("pack://application:,,,/assesst_shootergame/buff_damage.png"));
                    newBuff.Tag = new { Type = "damageBuff" };
                    break;
            }

            newBuff.Width = 30;
            newBuff.Height = 30;
            Canvas.SetLeft(newBuff, rand.Next(0, (int)GameCanvas.ActualWidth - 30));
            Canvas.SetTop(newBuff, -30);
            GameCanvas.Children.Add(newBuff);
        }
        private void GameCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            // Lấy vị trí chuột
            Point mousePosition = e.GetPosition(GameCanvas);

            // Cập nhật vị trí của PlayerShip
            double newLeft = mousePosition.X - (PlayerShip.Width / 2); // Đặt ship ở giữa chuột
            double newTop = mousePosition.Y - (PlayerShip.Height / 2); // Đặt ship ở giữa chuột

            // Đảm bảo không ra ngoài canvas
            if (newLeft < 0) newLeft = 0;
            if (newLeft + PlayerShip.Width > GameCanvas.ActualWidth) newLeft = GameCanvas.ActualWidth - PlayerShip.Width;
            if (newTop < 0) newTop = 0;
            if (newTop + PlayerShip.Height > GameCanvas.ActualHeight) newTop = GameCanvas.ActualHeight - PlayerShip.Height;

            // Cập nhật vị trí của PlayerShip
            Canvas.SetLeft(PlayerShip, newLeft);
            Canvas.SetTop(PlayerShip, newTop);
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left) moveLeft = true;
            if (e.Key == Key.Right) moveRight = true;
            if (e.Key == Key.Space) FireBullet();
            if (e.Key == Key.Escape)
            {
                PauseGame();
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left) moveLeft = false;
            if (e.Key == Key.Right) moveRight = false;
        }
        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            PauseGame();
            ResumeGame();
        }

        private void PauseGame()
        {
            gameTimer.Stop(); // Stop the game timer
            MessageBoxResult result = MessageBox.Show("Game Paused. Click OK to resume.", "Paused", MessageBoxButton.OK);

            // Check the result of the MessageBox
            if (result == MessageBoxResult.OK)
            {
                gameTimer.Start(); // Resume the game timer
                                   // Add any additional code to resume animations or game logic if needed
            }
        }


        private void ResumeGame()
        {
            gameTimer.Start();
            // Add any additional code to resume animations or game logic
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Close the game window
        }
    }
}