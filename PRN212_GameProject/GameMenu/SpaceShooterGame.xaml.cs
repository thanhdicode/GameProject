using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GameMenu
{
    /// <summary>
    /// Interaction logic for SpaceShooterGame.xaml
    /// </summary>
    public partial class SpaceShooterGame : Window
    {
        private DispatcherTimer gameTimer = new DispatcherTimer();
        private DispatcherTimer obstacleTimer = new DispatcherTimer();
        private DispatcherTimer bossTimer = new DispatcherTimer();
        private DispatcherTimer bossFireTimer = new DispatcherTimer();
        private DispatcherTimer bulletTimer = new DispatcherTimer();
        private DispatcherTimer buffTimer = new DispatcherTimer();
        private DispatcherTimer warningTimer = new DispatcherTimer();
        private TextBlock warningMessage;
        private Random rand = new Random();
        private bool moveLeft, moveRight;
        private int playerSpeed = 8;
        private int score = 0;
        private int playerHealth = 3;
        private bool bossAppeared = false;
        private bool canShoot = true;
        private int bossScoreMultiplier = 5;
        private double bossHealth = 300;
        private const double BossBulletSpeed = 5;
        private const int NumberOfBossBullets = 10;
        private double bossBulletAngleIncrement;
        private int currentBossBulletIndex = 0;
        private double bossBulletBaseAngle = 0;
        private TextBlock bossMessage;
        private int maxHealth = 3; // Maximum health
        private int bossShotsFired = 0; // To keep track of the number of shots fired by the boss
        private DispatcherTimer bossFireDelayTimer = new DispatcherTimer(); // Timer for delay after two shots
        private int ammoBuffCount = 0; // Biến để theo dõi số lần nhận buff ammo
        private int level = 1; // Initialize level

        public SpaceShooterGame()
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

            bulletTimer.Interval = TimeSpan.FromMilliseconds(500);
            bulletTimer.Tick += (s, e) => canShoot = true;
            bulletTimer.Start();
            buffTimer.Interval = TimeSpan.FromSeconds(15);
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
                    if (tag.Type == "buff_ammo" || tag.Type == "buff_health" || tag.Type == "buff_damage")
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
                case "buff_ammo":
                    // Increase the firing rate by 10%
                    bulletTimer.Interval = TimeSpan.FromMilliseconds(bulletTimer.Interval.TotalMilliseconds * 0.9);
                    break;

                case "buff_health":
                    // Increase player health
                    if (playerHealth < maxHealth)
                    {
                        playerHealth++;
                        UpdateScoreAndHealth();
                    }
                    break;

                case "buff_damage":
                    // Increase bullet damage by 10%
                    foreach (var bullet in GameCanvas.Children.OfType<Image>().Where(r => (r.Tag as dynamic)?.Type == "bullet"))
                    {
                        var tag = (dynamic)bullet.Tag;
                        var newTag = new { Type = tag.Type, Damage = tag.Damage * 1.1 }; // Increase damage by 10%
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
                double bulletSpeed = 5; // Tốc độ đạn mới

                if (ammoBuffCount == 1)
                {
                    Image newBullet = new Image
                    {
                        Source = new BitmapImage(new Uri("pack://application:,,,/assesst_shootergame/buff_ammo.png")),
                        Width = 40,
                        Height = 40,
                        Tag = new { Type = "bullet", Damage = 5, Speed = bulletSpeed }
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
                        Tag = new { Type = "bullet", Damage = 8, Speed = bulletSpeed }
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

            // List of obstacle image paths
            string[] obstacleImages = new string[]
            {
                "pack://application:,,,/assesst_shootergame/obstancle.png",
                "pack://application:,,,/assesst_shootergame/obstancle1.png",
                "pack://application:,,,/assesst_shootergame/obstancle2.png"
            };

            // Randomly select one of the obstacle images
            string selectedImage = obstacleImages[rand.Next(obstacleImages.Length)];

            Image newObstacle = new Image
            {
                Source = new BitmapImage(new Uri(selectedImage)),
                Width = 50,
                Height = 50,
                Tag = new { Type = "obstacle", Health = rand.Next(10, 16) }
            };

            Canvas.SetLeft(newObstacle, rand.Next(0, (int)GameCanvas.ActualWidth - 50));
            Canvas.SetTop(newObstacle, -50);
            GameCanvas.Children.Add(newObstacle);
        }

        private void ShowWarningMessage()
        {
            if (warningMessage == null)
            {
                int countdownSeconds = 10;

                warningMessage = new TextBlock
                {
                    FontSize = 20,
                    Foreground = Brushes.Red,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                // Add the warning message to the canvas
                GameCanvas.Children.Add(warningMessage);

                // Timer for updating the countdown
                var countdownTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1)
                };
                countdownTimer.Tick += (s, e) =>
                {
                    if (countdownSeconds > 0)
                    {
                        warningMessage.Text = $"Boss'll appear in {countdownSeconds} seconds!";
                        countdownSeconds--;
                    }
                    else
                    {
                        countdownTimer.Stop();
                        warningMessage.Visibility = Visibility.Collapsed;
                        warningMessage = null;

                        // Trigger boss appearance logic here if needed
                        // For example: SpawnBoss();
                    }
                };
                countdownTimer.Start();

                // Initial message setup
                warningMessage.Text = $"Boss'll appear in {countdownSeconds} seconds!";

                // Force a layout update to get accurate dimensions
                warningMessage.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                warningMessage.Arrange(new Rect(new Size(GameCanvas.ActualWidth, GameCanvas.ActualHeight)));

                Canvas.SetLeft(warningMessage, (GameCanvas.ActualWidth - warningMessage.ActualWidth) / 2);
                Canvas.SetTop(warningMessage, (GameCanvas.ActualHeight - warningMessage.ActualHeight) / 2);
            }
        }




        private void PrepareBossAppearance()
        {
            // Show warning message 10 seconds before boss appears
            ShowWarningMessage();

            // Set up a timer to actually prepare the boss appearance after 10 seconds
            warningTimer.Interval = TimeSpan.FromSeconds(10);
            warningTimer.Tick += (s, e) =>
            {
                warningTimer.Stop(); // Stop the warning timer

                // Proceed to prepare the boss appearance
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

                    // List of boss image paths
                    string[] bossImages = new string[]
                    {
                        "pack://application:,,,/assesst_shootergame/boss.png",
                        "pack://application:,,,/assesst_shootergame/boss1.png",
                        "pack://application:,,,/assesst_shootergame/boss2.png"
                    };

                    // Randomly select one of the boss images
                    string selectedBossImage = bossImages[rand.Next(bossImages.Length)];

                    // Create the boss
                    Image boss = new Image
                    {
                        Source = new BitmapImage(new Uri(selectedBossImage)),
                        Width = 100,
                        Height = 100,
                        Tag = new { Type = "boss", Health = bossHealth }
                    };
                    Canvas.SetLeft(boss, (GameCanvas.ActualWidth - boss.Width) / 2);
                    Canvas.SetTop(boss, 30);
                    GameCanvas.Children.Add(boss);

                    // Update boss health for next appearance
                    bossHealth *= 1.5; // Increase boss health for the next boss

                    // Remove boss message
                    if (bossMessage != null)
                    {
                        GameCanvas.Children.Remove(bossMessage);
                        bossMessage = null;
                    }

                    // Set up boss shooting
                    bossFireTimer.Interval = TimeSpan.FromSeconds(0.5); // Adjust as needed
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

                    // Update level
                    level++;
                    LevelTextBlock.Text = $"Level: {level}";
                }
            };
            warningTimer.Start();
        }





        private void FireBossBullets(Image boss)
        {
            int numberOfBullets = NumberOfBossBullets;
            double screenWidth = GameCanvas.ActualWidth;
            double screenHeight = GameCanvas.ActualHeight;
            double targetX = screenWidth / 2; // Giữa màn hình theo chiều ngang
            double targetY = screenHeight; // Dưới cùng màn hình theo chiều dọc

            Random random = new Random();

            for (int i = 0; i < numberOfBullets; i++)
            {
                double angle, radians, xSpeed, ySpeed;

                // Tính toán góc hướng về điểm mục tiêu (giữa dưới màn hình)
                double bossX = Canvas.GetLeft(boss) + boss.Width / 2;
                double bossY = Canvas.GetTop(boss) + boss.Height / 2;
                double angleToTarget = Math.Atan2(targetY - bossY, targetX - bossX) * (180 / Math.PI);

                switch (random.Next(3))
                {
                    case 0: // Bắn ngẫu nhiên với lệch nhỏ
                        angle = angleToTarget + (random.NextDouble() - 0.5) * 30; // Lệch ngẫu nhiên trong khoảng -15 đến 15 độ
                        break;

                    case 1: // Bắn theo hình tròn với chút ngẫu nhiên
                        angle = angleToTarget + (360.0 / numberOfBullets) * i + random.NextDouble() * 10 - 5; // Lệch ngẫu nhiên nhỏ
                        break;

                    case 2: // Bắn theo tỏa ra với chút ngẫu nhiên
                        angle = angleToTarget + (360.0 / numberOfBullets) * i + random.NextDouble() * 10 - 5; // Lệch ngẫu nhiên nhỏ
                        break;

                    default:
                        throw new InvalidOperationException("Unexpected shooting pattern.");
                }

                radians = angle * (Math.PI / 180);
                xSpeed = BossBulletSpeed * Math.Cos(radians);
                ySpeed = BossBulletSpeed * Math.Sin(radians);

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

            bossBulletBaseAngle += 5;
            if (bossBulletBaseAngle >= 360)
            {
                bossBulletBaseAngle -= 360;
            }
            bossShotsFired++;

            if (bossShotsFired % 2 == 0)
            {
                bossFireTimer.Stop();
                bossFireDelayTimer.Interval = TimeSpan.FromSeconds(4); // Delay 1 giây trước khi bắn tiếp
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
            // Dừng tất cả các bộ đếm thời gian
            gameTimer.Stop();
            obstacleTimer.Stop();
            bossTimer.Stop();
            bossFireTimer.Stop();
            bulletTimer.Stop();
            buffTimer.Stop(); // Dừng bộ đếm thời gian buff

            // Dừng mọi điều khiển
            moveLeft = false;
            moveRight = false;

            // Loại bỏ các sự kiện điều khiển
            this.MouseMove -= Window_MouseMove;

            // Vô hiệu hóa tàu vũ trụ
            PlayerShip.IsHitTestVisible = false;

            // Cập nhật điểm số cuối cùng
            FinalScoreTextBlock.Text = $"Final Score: {score}";

            // Hiển thị lớp phủ kết thúc trò chơi
            ShowGameOverOverlay();
        }


        private void ShowGameOverOverlay()
        {
            // Set the overlay to visible
            GameOverOverlay.Visibility = Visibility.Visible;

            // Set width and height for the overlay grid
            double overlayWidth = 300; // Adjust as needed
            double overlayHeight = 200; // Adjust as needed

            // Update size for the overlay grid
            GameOverOverlay.Width = overlayWidth;
            GameOverOverlay.Height = overlayHeight;

            // Center the overlay within the canvas
            double canvasWidth = GameCanvas.ActualWidth;
            double canvasHeight = GameCanvas.ActualHeight;

            // Ensure the overlay is centered
            Canvas.SetLeft(GameOverOverlay, (canvasWidth - overlayWidth) / 2);
            Canvas.SetTop(GameOverOverlay, (canvasHeight - overlayHeight) / 2);
        }



        private void PlayAgain_Click(object sender, RoutedEventArgs e)
        {
            RestartGame();
            this.MouseMove += Window_MouseMove;
            // Hide the game over overlay
            GameOverOverlay.Visibility = Visibility.Collapsed;
            LevelTextBlock.Visibility = Visibility.Visible;
            HealthTextBlock.Visibility = Visibility.Visible;
            ScoreTextBlock.Visibility = Visibility.Visible;
            // Restart the game
        }

        private void RestartGame()
        {
            // Clear existing game content
            GameCanvas.Children.Clear();

            // Reset game state
            level = 1;
            score = 0;
            maxHealth = 3;

            // Create and add UI elements back to the canvas
            CreateUIElements();

            // Reinitialize player ship
            PlayerShip = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/assesst_shootergame/ship.png")),
                Width = 50,
                Height = 50
            };
            Canvas.SetLeft(PlayerShip, (GameCanvas.ActualWidth - PlayerShip.Width) / 2);
            Canvas.SetTop(PlayerShip, GameCanvas.ActualHeight - PlayerShip.Height - 10); // Position it at the bottom

            // Add player ship to the canvas
            GameCanvas.Children.Add(PlayerShip);

            // Restart timers
            gameTimer.Start();
            obstacleTimer.Start();
            bulletTimer.Start();
            buffTimer.Start();

            // Remove any existing boss messages or boss objects
            var bossMessage = GameCanvas.Children.OfType<TextBlock>().FirstOrDefault(t => t.Text == "BOSS!!!");
            if (bossMessage != null)
            {
                GameCanvas.Children.Remove(bossMessage);
            }
            var bossImage = GameCanvas.Children.OfType<Image>().FirstOrDefault(i => (i.Tag as dynamic)?.Type == "boss");
            if (bossImage != null)
            {
                GameCanvas.Children.Remove(bossImage);
            }

            // Reset boss-related timers
            bossFireTimer.Stop();
            bossFireDelayTimer.Stop();

            // Restart the boss appearance timer
            bossTimer.Start();
        }

        private void CreateUIElements()
        {
            // Add Score TextBlock
            var scoreTextBlock = new TextBlock
            {
                Name = "ScoreTextBlock",
                Text = $"Score: {score}",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White
            };
            Canvas.SetLeft(scoreTextBlock, 10);
            Canvas.SetTop(scoreTextBlock, 10);
            GameCanvas.Children.Add(scoreTextBlock);

            // Add Health TextBlock
            var healthTextBlock = new TextBlock
            {
                Name = "HealthTextBlock",
                Text = $"Health: {maxHealth}",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White
            };
            Canvas.SetLeft(healthTextBlock, 10);
            Canvas.SetTop(healthTextBlock, 40);
            GameCanvas.Children.Add(healthTextBlock);

            // Add Level TextBlock
            var levelTextBlock = new TextBlock
            {
                Name = "LevelTextBlock",
                Text = $"Level: {level}",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White
            };
            Canvas.SetLeft(levelTextBlock, 10);
            Canvas.SetTop(levelTextBlock, 70);
            GameCanvas.Children.Add(levelTextBlock);
        }

        private void CreateBuff(object sender, EventArgs e)
        {
            // Decide the type of buff to create
            string buffType;
            double buffChance = rand.NextDouble(); // Generate a random number between 0 and 1

            // Determine which buff to create based on buffChance
            if (buffChance < 0.33) // 33% chance for health buff
            {
                if (playerHealth < maxHealth) // Check if health buff should be created
                {
                    buffType = "buff_health";
                }
                else
                {
                    // Skip creating health buff if player is at max health
                    buffType = GetRandomBuffType(); // Get a different buff type
                }
            }
            else if (buffChance < 0.66) // 33% chance for ammo buff
            {
                buffType = "buff_ammo";
            }
            else // 34% chance for damage buff
            {
                buffType = "buff_damage";
            }

            // Create the buff
            Image newBuff = new Image
            {
                Source = new BitmapImage(new Uri($"pack://application:,,,/assesst_shootergame/{buffType}.png")),
                Width = 40,
                Height = 40,
                Tag = new { Type = buffType }
            };
            Canvas.SetLeft(newBuff, rand.Next(0, (int)GameCanvas.ActualWidth - 40));
            Canvas.SetTop(newBuff, -40);
            GameCanvas.Children.Add(newBuff);
        }

        // Helper method to get a random buff type
        private string GetRandomBuffType()
        {
            double chance = rand.NextDouble();
            if (chance < 0.5)
            {
                return "buff_ammo";
            }
            else
            {
                return "buff_damage";
            }
        }




        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePosition = e.GetPosition(GameCanvas);

            // Update player ship's position to follow the mouse's X coordinate
            double newLeft = mousePosition.X - (PlayerShip.Width / 2);

            // Ensure the ship stays within the bounds of the canvas
            if (newLeft >= 0 && newLeft <= GameCanvas.ActualWidth - PlayerShip.Width)
            {
                Canvas.SetLeft(PlayerShip, newLeft);
            }
        }
    }
}
