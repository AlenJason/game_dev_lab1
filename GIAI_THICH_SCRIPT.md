# Giải thích chi tiết các script – Lab 1 Space Shooter

Tài liệu này giải thích từng script trong `Assets/Scripts` (và công cụ `Assets/Editor/BuildLab1.cs`): script đó gắn vào GameObject nào, có những biến gì, từng hàm làm gì và nó nói chuyện với script nào khác.

---

## 1. Tổng quan

### Hai scene

| Scene | Vai trò |
|---|---|
| `Lobby.unity` (build index 0) | Màn hình chờ: nút **PLAY**, **TUTORIAL**, **HISTORY**, hiện **Best Score** |
| `SpaceScene.unity` (build index 1) | Màn chơi: map, tàu, thiên thạch, sao, tàu địch, boss, HUD |

### Các GameObject chính trong `SpaceScene`

| GameObject | Script / Component |
|---|---|
| `Main Camera` | Camera (orthographic size 5) + **CameraFollow** |
| `Map/Background` | SpriteRenderer (chế độ Tiled, 40×24) |
| `Map/Walls/Top, Bottom, Left, Right` | BoxCollider2D (tag `Wall`, Physics Material `Bouncy`) |
| `Spaceship` (tag `Player`) | SpriteRenderer, Rigidbody2D, PolygonCollider2D, **PlayerHealth**, **PlayerController** |
| `Spaceship/Gun` | **Gun** |
| `GameManager` | **GameManager**, **UpgradeManager** |
| `Spawner` | **Spawner** |
| `WaveManager` | **WaveManager** |
| `Canvas` | Canvas + **HUD** (điểm, level, wave, 3 tim, thanh máu boss, banner, bảng chọn kỹ năng, bảng Game Over) |
| `EventSystem` | Để bấm được các nút UI |

### Các prefab (`Assets/Prefabs`)

| Prefab | Tag | Component |
|---|---|---|
| `Bullet` | – | SpriteRenderer, Rigidbody2D, BoxCollider2D (trigger), **Bullet** |
| `EnemyBullet` | – | SpriteRenderer, Rigidbody2D, CircleCollider2D (trigger), **EnemyBullet** |
| `Asteroid` | `Asteroid` | SpriteRenderer, Rigidbody2D, CircleCollider2D (Bouncy), **Asteroid** |
| `Star` | `Star` | SpriteRenderer, Rigidbody2D (Kinematic), CircleCollider2D (trigger), **Star** |
| `Enemy` | `Enemy` | SpriteRenderer, Rigidbody2D, CircleCollider2D, **Enemy** |
| `Boss` | `Boss` | SpriteRenderer, Rigidbody2D (Kinematic), CircleCollider2D, **Boss** |

### Sơ đồ các script nói chuyện với nhau

```
LobbyMenu ──(PLAY)──► SpaceScene
   ▲                      │
   │ đọc lịch sử          ▼
RunHistory ◄──lưu── GameManager ◄── AddScore / EnemyKilled / BossDefeated ── Asteroid, Star, Enemy, Boss
                       │   │
              mở bảng  │   └──► HUD (điểm, level, tim, wave, boss bar)
                       ▼
               UpgradeManager ──nâng cấp──► Gun, PlayerController, PlayerHealth

WaveManager ──Spawn──► Spawner ──Instantiate──► Asteroid / Star / Enemy / Boss
Gun ──Instantiate──► Bullet ──TakeDamage──► IDamageable (Asteroid, Enemy, Boss)
Enemy, Boss ──EnemyBullet.Fire──► EnemyBullet ──TakeHit──► PlayerHealth
```

### Một vài kiến thức Unity được dùng xuyên suốt

- **`[SerializeField]`**: biến `private` nhưng vẫn hiện trong Inspector để chỉnh / kéo thả.
- **Singleton `GameManager.Instance`**: chỉ có 1 GameManager, script nào cũng gọi được qua `GameManager.Instance`.
- **`Update()`** chạy mỗi frame (đọc phím, hẹn giờ); **`FixedUpdate()`** chạy theo nhịp vật lý (đặt vận tốc Rigidbody2D).
- **`OnCollisionEnter2D`**: 2 collider thường va nhau. **`OnTriggerEnter2D`**: có ít nhất 1 collider là *trigger* (đi xuyên qua, chỉ báo sự kiện).
- **Tag** (`Player`, `Asteroid`, `Star`, `Enemy`, `Boss`, `Wall`) dùng để phân biệt vật bị va chạm: `CompareTag("Asteroid")`.
- **Coroutine** (`IEnumerator` + `yield return`): viết code chạy kéo dài nhiều frame, ví dụ "đợi 2 giây rồi làm tiếp".
- **`Time.timeScale = 0`** làm game dừng (dùng khi hiện bảng chọn kỹ năng), `= 1` là chạy bình thường.

---

## 2. Script của màn chơi

### 2.1 `PlayerController.cs` – Điều khiển tàu

**Gắn vào:** `Spaceship`. `[RequireComponent(typeof(Rigidbody2D), typeof(PlayerHealth))]` bắt buộc GameObject phải có 2 component này.

| Biến | Ý nghĩa |
|---|---|
| `moveSpeed = 6` | Tốc độ bay. Có property `MoveSpeed` để `UpgradeManager` tăng khi chọn **ENGINE BOOST** |

**Các hàm:**
- `Start()`: lấy `Rigidbody2D` và `PlayerHealth` của tàu.
- `Update()`:
  1. Nếu đã Game Over hoặc game đang dừng (`Time.timeScale == 0`) thì bỏ qua.
  2. Đọc phím: `Input.GetAxisRaw("Horizontal"/"Vertical")` (WASD / mũi tên), `.normalized` để đi chéo không nhanh hơn đi thẳng.
  3. Xoay tàu theo chuột: đổi vị trí chuột trên màn hình sang toạ độ thế giới (`Camera.main.ScreenToWorldPoint`), tính góc bằng `Mathf.Atan2(y, x)`, **trừ 90°** vì sprite tàu vẽ mũi hướng lên trên (trục Y), còn `Atan2` tính góc so với trục X.
- `FixedUpdate()`: đặt `rb.linearVelocity = moveInput * moveSpeed` (đã Game Over thì đứng yên).
- `OnCollisionEnter2D()`:
  - Va **thiên thạch** hoặc **tàu địch** → `health.TakeHit()`. Nếu đòn có tác dụng (không đang bất tử) thì phá huỷ luôn vật đó.
  - Va **boss** → chỉ mất 1 tim (boss không bị phá huỷ).
- `OnTriggerEnter2D()`: chạm **sao** (trigger) → cộng điểm `Star.ScoreValue` và xoá sao.

### 2.2 `PlayerHealth.cs` – 3 mạng (3 trái tim)

**Gắn vào:** `Spaceship`.

| Biến | Ý nghĩa |
|---|---|
| `maxLives = 3` | Số tim tối đa |
| `invincibleTime = 2` | Số giây bất tử sau khi trúng đòn |
| `hud` | Tham chiếu HUD để vẽ lại trái tim |
| `Lives` (property) | Số tim hiện tại |

**Các hàm:**
- `Start()`: `Lives = maxLives`, cập nhật 3 trái tim trên HUD.
- `TakeHit()` – được gọi khi trúng đạn / va chạm:
  1. Đang bất tử hoặc đã Game Over → trả về `false` (không mất tim).
  2. `Lives--` và cập nhật HUD.
  3. Hết tim → `GameManager.Instance.GameOver()` và ẩn tàu. Còn tim → chạy coroutine bất tử.
  4. Trả về `true` để người gọi biết đòn đã có tác dụng.
- `Heal(amount)`: hồi tim, không vượt `maxLives` (`Mathf.Min`). Được gọi khi chọn kỹ năng **REPAIR** hoặc khi hạ boss.
- `InvincibleRoutine()` (coroutine): trong 2 giây cứ 0.1 giây bật/tắt `spriteRenderer.enabled` → tàu **nhấp nháy**; hết giờ thì hiện lại và hết bất tử.

### 2.3 `Gun.cs` – Súng

**Gắn vào:** `Spaceship/Gun` (GameObject con đặt ở mũi tàu, `localPosition (0, 0.5)`). Vì là con của tàu nên nó xoay theo tàu.

| Biến | Ý nghĩa | Kỹ năng nâng cấp |
|---|---|---|
| `bulletPrefab` | Prefab `Bullet` | – |
| `fireRate = 5` | Số lượt bắn mỗi giây | RAPID FIRE (×1.3) |
| `bulletCount = 1` | Số tia mỗi lượt | MULTI SHOT (+1, tối đa 5) |
| `damage = 1` | Sát thương mỗi viên | POWER SHOT (+1, tối đa 5) |
| `spreadAngle = 12` | Góc giữa 2 tia | – |

**Các hàm:**
- `Update()`: giữ **chuột trái** hoặc **Space** và đã tới lượt (`Time.time >= nextFireTime`) thì bắn, rồi hẹn lượt sau `nextFireTime = Time.time + 1 / fireRate`. Đây là kỹ thuật **cooldown** bằng thời gian.
- `Fire()`: bắn `bulletCount` viên toả đều quanh hướng mũi tàu. Góc bắt đầu `-spreadAngle * (bulletCount - 1) / 2`, mỗi viên lệch thêm `spreadAngle`. Ví dụ 3 tia → -12°, 0°, +12°. Mỗi viên được gán `Damage`.

### 2.4 `Bullet.cs` – Đạn của tàu

**Gắn vào:** prefab `Bullet` (collider là **trigger**).

| Biến | Ý nghĩa |
|---|---|
| `speed = 14` | Tốc độ bay |
| `lifeTime = 2` | Tự huỷ sau 2 giây (tránh đầy bộ nhớ) |
| `Damage` | Do `Gun` gán khi bắn |

**Các hàm:**
- `Start()`: vận tốc = `transform.up * speed` (bay theo hướng mũi đạn), `Destroy(gameObject, lifeTime)`.
- `OnTriggerEnter2D()`:
  - Trúng **tường** → huỷ đạn.
  - Vật trúng có script cài **`IDamageable`** (thiên thạch, tàu địch, boss) → gọi `TakeDamage(Damage)` rồi huỷ đạn. Nhờ interface nên đạn không cần biết vật đó cụ thể là gì.

### 2.5 `IDamageable.cs` – Interface "có thể bị bắn"

```csharp
public interface IDamageable { void TakeDamage(int damage); }
```

`Asteroid`, `Enemy`, `Boss` đều cài interface này, mỗi class tự quyết định bị bắn thì ra sao (thiên thạch vỡ ngay, tàu địch trừ máu, boss trừ máu và cập nhật thanh máu). Đây là ví dụ về **tính đa hình (polymorphism)** trong lập trình hướng đối tượng.

### 2.6 `Asteroid.cs` – Thiên thạch

**Gắn vào:** prefab `Asteroid`.

| Biến | Ý nghĩa |
|---|---|
| `sprites` | 5 hình thiên thạch để chọn ngẫu nhiên |
| `minSpeed = 1`, `maxSpeed = 3` | Khoảng tốc độ ngẫu nhiên |
| `maxSpin = 90` | Tốc độ xoay tối đa (độ/giây) |
| `scoreValue = 10` | Điểm khi bị bắn vỡ |

**Các hàm:**
- `Start()` – mọi thứ đều **ngẫu nhiên** (đúng yêu cầu đề):
  - Hình: `sprites[Random.Range(0, sprites.Length)]`, rồi chỉnh bán kính `CircleCollider2D` cho khớp kích thước hình vừa chọn.
  - Hướng: `Random.insideUnitCircle.normalized` (một hướng bất kỳ trên vòng tròn).
  - Tốc độ: `Random.Range(minSpeed, maxSpeed)`; xoay: `angularVelocity = Random.Range(-maxSpin, maxSpin)`.
- **Nảy khi chạm biên map**: không cần code. Collider của thiên thạch và tường đều dùng Physics Material `Bouncy` (friction 0, bounciness 1), nên engine vật lý tự làm nảy.
- `TakeDamage()`: cộng điểm và huỷ. Biến `isDestroyed` chặn trường hợp 2 viên đạn trúng cùng lúc làm cộng điểm 2 lần.

### 2.7 `Star.cs` – Ngôi sao

**Gắn vào:** prefab `Star`. Rigidbody2D **Kinematic** (không chịu lực, không va chạm vật lý) + collider **trigger** (để tàu nhặt được).

| Biến | Ý nghĩa |
|---|---|
| `speed = 1.5` | Tốc độ trôi |
| `changeDirectionTime = 2` | Cứ 2 giây đổi hướng ngẫu nhiên |
| `spinSpeed = 60` | Tốc độ xoay |
| `scoreValue = 5` | Điểm khi nhặt |

**Các hàm:**
- `Start()`: bắt đầu xoay, chọn hướng ngẫu nhiên.
- `Update()`: đếm ngược `timer`, hết giờ thì `PickRandomDirection()`. Vì Kinematic không va tường, nên script tự kiểm tra: ra khỏi `GameManager.Instance.MapBounds` thì **đảo dấu vận tốc** (quay đầu).
- `PickRandomDirection()`: `rb.linearVelocity = Random.insideUnitCircle.normalized * speed`.

### 2.8 `Spawner.cs` – Sinh thiên thạch và sao

**Gắn vào:** `Spawner`.

| Biến | Ý nghĩa |
|---|---|
| `asteroidPrefab`, `starPrefab` | Prefab cần sinh |
| `player` | Transform của tàu (để không sinh đè lên tàu) |
| `maxAsteroids = 18`, `maxStars = 6` | Số lượng tối đa cùng lúc trên map |
| `spawnInterval = 1` | Cứ 1 giây kiểm tra và sinh thêm |
| `safeDistance = 5` | Khoảng cách tối thiểu so với tàu |

**Các hàm:**
- `Start()`: sinh sẵn một nửa số lượng tối đa.
- `Update()`: mỗi `spawnInterval` giây, nếu số thiên thạch/sao (đếm bằng `FindGameObjectsWithTag`) còn ít hơn mức tối đa thì sinh thêm 1.
- `Spawn(prefab, minDistance)` (public, `WaveManager` cũng dùng):
  - Chọn vị trí ngẫu nhiên trong `MapBounds`.
  - Nếu quá gần tàu thì chọn lại (vòng `do...while`, tối đa 50 lần để không bị treo).
  - Gọi `Instantiate`, trả về GameObject vừa sinh.

### 2.9 `WaveManager.cs` – Chế độ wave và boss

**Gắn vào:** `WaveManager`.

| Biến | Ý nghĩa |
|---|---|
| `bossEveryWaves = 5` | Cứ 5 wave có 1 wave boss |
| `firstWaveEnemies = 3` | Số địch wave 1 |
| `extraEnemiesPerWave = 2` | Mỗi wave thêm 2 địch |
| `enemySpawnDelay = 0.8` | Khoảng cách giữa 2 lần sinh địch trong 1 wave |
| `timeBetweenWaves = 3` | Nghỉ giữa 2 wave |
| `Wave`, `IsBossWave` | Wave hiện tại, có phải wave boss không |

**Luật wave:**
- Số địch của wave thường = `3 + (wave - 1) × 2` → 3, 5, 7, 9, (boss), 13, 15...
- Wave chia hết cho 5 (5, 10, 15...) là **wave boss**: **chỉ có boss**, không sinh tàu địch con. Boss thứ `wave / 5` mạnh hơn boss trước.

**Các hàm:**
- `Start()`: chạy coroutine `RunWaves()`.
- `Update()`: cập nhật dòng `WAVE 3 - ENEMIES: 5` trên HUD (số địch = chưa sinh + đang bay).
- `RunWaves()` (vòng lặp chính, viết bằng coroutine cho dễ đọc):
  1. `Wave++`, xác định có phải wave boss.
  2. Hiện banner `WAVE n` / `BOSS WAVE n`, đợi 2 giây.
  3. Chạy `BossWave()` hoặc `EnemyWave()` và **đợi tới khi wave xong**.
  4. Hiện `WAVE n CLEAR!`, nghỉ 3 giây, lặp lại.
- `EnemyWave(count)`: sinh từng tàu địch, mỗi tàu gọi `Enemy.Setup(Wave)` để tăng độ khó. Sau đó đợi tới khi không còn GameObject tag `Enemy`.
- `BossWave(bossNumber)`: sinh boss cách tàu ít nhất 8 đơn vị, gọi `Boss.Setup(bossNumber, hud)`, đợi tới khi `boss == null` (Unity coi object đã bị `Destroy` là null).

`WaitForSeconds` phụ thuộc `Time.timeScale`, nên khi đang mở bảng chọn kỹ năng (timeScale = 0) thì wave cũng tạm dừng.

### 2.10 `Enemy.cs` – Tàu địch

**Gắn vào:** prefab `Enemy`.

| Biến | Ý nghĩa |
|---|---|
| `sprites` | 3 hình tàu địch chọn ngẫu nhiên |
| `enemyBulletPrefab` | Đạn của địch |
| `moveSpeed = 2.5` | Tốc độ đuổi |
| `keepDistance = 3` | Tới gần tàu người chơi 3 đơn vị thì dừng lại (để bắn) |
| `fireInterval = 2`, `fireRange = 9` | Bắn mỗi 2 giây nếu cách ≤ 9 đơn vị |
| `maxHealth = 2`, `scoreValue = 20` | Máu và điểm khi bị hạ |

**Các hàm:**
- `Setup(wave)` – `WaveManager` gọi ngay sau khi sinh:
  - Máu: `+ wave / 3`.
  - Tốc độ: `+ 0.1 × wave` (tối đa 5).
  - Thời gian giữa 2 lần bắn: `- 0.08 × wave` (tối thiểu 0.8 giây).
- `Start()`: chọn hình ngẫu nhiên, tìm tàu người chơi (`FindGameObjectWithTag("Player")`), hẹn phát bắn đầu tiên ngẫu nhiên để các tàu địch không bắn cùng lúc.
- `FixedUpdate()`: xoay mũi về phía tàu người chơi (**cộng 90°** vì sprite tàu địch vẽ hướng xuống dưới), bay tới nếu còn xa hơn `keepDistance`.
- `Update()`: trong tầm bắn và tới lượt thì `EnemyBullet.Fire(...)` về phía người chơi.
- `TakeDamage()`: trừ máu, tàu đỏ dần theo lượng máu đã mất (`Color.Lerp`). Hết máu → `GameManager.Instance.EnemyKilled(20)` và huỷ.

### 2.11 `EnemyBullet.cs` – Đạn của địch và boss

**Gắn vào:** prefab `EnemyBullet` (trigger).

- `Fire(prefab, position, direction)` (**static**, gọi được mà không cần object): tạo viên đạn, xoay sprite theo hướng bay, đặt vận tốc `direction * speed` (`speed = 6`).
- `Start()`: tự huỷ sau 4 giây.
- `OnTriggerEnter2D()`: trúng **Player** → `PlayerHealth.TakeHit()` rồi huỷ; trúng **tường** → huỷ.

### 2.12 `Boss.cs` – Boss

**Gắn vào:** prefab `Boss`. Rigidbody2D **Kinematic** (di chuyển bằng code `MovePosition`, không bị đẩy). `useFullKinematicContacts` bật để vẫn báo va chạm với tàu người chơi.

| Biến | Giá trị boss 1 | Mỗi boss sau |
|---|---|---|
| `baseHealth` / `healthPerBoss` | 40 máu | +30 máu |
| `moveSpeed` | 2 | +0.3 |
| `burstBullets` (đạn toả tròn) | 10 viên | +2 (tối đa 24) |
| `burstInterval` | 3.5 giây | ×0.9 (nhanh hơn 10%) |
| `aimedInterval` (3 tia nhắm) | 1.6 giây | ×0.9 |
| Kích thước | 2.2 | +10% (tối đa +40%) |
| `scoreValue` | 200 điểm | – |

**Các hàm:**
- `Setup(bossNumber, hud)`: tính các chỉ số theo bảng trên, hiện thanh máu `BOSS n`.
- `IsEnraged` (property): máu ≤ 50% → **nổi giận**: lượn nhanh hơn, bắn dày hơn (khoảng cách giữa các loạt đạn ×0.6), sprite ửng đỏ.
- `FixedUpdate()`: **lượn vòng quanh người chơi**. Góc `orbitAngle` tăng dần, vị trí mục tiêu = vị trí người chơi + vector bán kính 6 xoay theo góc đó, kẹp trong map (`Mathf.Clamp`), rồi `MoveTowards` tới đó.
- `Update()`: hai bộ hẹn giờ cho hai kiểu tấn công.
  - `FireBurst()`: `burstBullets` viên chia đều 360°, góc bắt đầu ngẫu nhiên. Toạ độ hướng tính bằng `Cos`/`Sin`.
  - `FireAimed()`: 3 tia về phía người chơi, lệch -15°, 0°, +15°.
- `TakeDamage()`: trừ máu, cập nhật thanh máu boss. Hết máu → `GameManager.Instance.BossDefeated(200)` (cộng điểm, hồi 1 tim) và huỷ.

### 2.13 `GameManager.cs` – Bộ não của màn chơi

**Gắn vào:** `GameManager`. Là **singleton** (`Instance`).

| Biến / Property | Ý nghĩa |
|---|---|
| `hud`, `upgradeManager`, `waveManager` | Các script khác cần điều khiển |
| `gameOverPanel`, `gameOverScoreText` | Bảng Game Over |
| `mapSize = (40, 24)` | Kích thước map |
| `MapBounds` | Vùng bên trong map (chừa lề 1 đơn vị), dùng để sinh vật và giới hạn sao/boss |
| `IsGameOver`, `Level` | Trạng thái game |

**Cách tính level** – theo tổng điểm:

```csharp
static int ScoreForLevel(int level) => 40 * level * (level - 1);
// Level 2: 80, Level 3: 240, Level 4: 480, Level 5: 800, Level 6: 1200 ...
```

**Các hàm:**
- `Awake()`: gán `Instance`, đặt `Time.timeScale = 1` (phòng trường hợp chơi lại lúc game đang dừng).
- `AddScore(amount)`:
  - Cộng điểm, cập nhật HUD.
  - Vòng `while` kiểm tra đã vượt mốc level tiếp theo chưa. Vượt nhiều mốc một lúc thì lên nhiều level, mỗi level được chọn 1 kỹ năng (`pendingLevelUps`).
  - Mở bảng chọn kỹ năng nếu chưa mở.
- `OnUpgradeChosen()`: `UpgradeManager` gọi sau khi chọn xong. Còn lượt lên level thì mở bảng tiếp.
- `EnemyKilled()`, `BossDefeated()`: đếm số địch/boss đã hạ (để lưu lịch sử), cộng điểm. Hạ boss thì ẩn thanh máu boss và hồi 1 tim.
- `GameOver()`:
  - Lưu lượt chơi bằng `RunHistory.Save(...)` (điểm, wave, level, số địch, số boss).
  - Hiện `NEW BEST!` nếu phá kỷ lục, nếu không thì hiện `BEST: ...`.
  - Mở bảng Game Over.
- `PlayAgain()` / `BackToLobby()`: gắn vào 2 nút trên bảng Game Over, nạp lại scene hoặc về `Lobby`.

### 2.14 `UpgradeManager.cs` – Chọn kỹ năng khi lên level

**Gắn vào:** `GameManager`.

**5 kỹ năng** – mỗi kỹ năng có `title`, `description`, điều kiện `isAvailable` (còn nâng được không) và hành động `apply`:

| Kỹ năng | Tác dụng | Giới hạn |
|---|---|---|
| RAPID FIRE | Bắn nhanh hơn 30% | `fireRate < 14` |
| MULTI SHOT | Thêm 1 tia đạn | tối đa 5 tia |
| POWER SHOT | Sát thương +1 | tối đa 5 |
| ENGINE BOOST | Tốc độ bay +1 | tối đa 10 |
| REPAIR | Hồi 1 trái tim | chỉ hiện khi chưa đủ 3 tim |

**Các hàm:**
- `Awake()`: ẩn bảng, tạo danh sách kỹ năng. `Func<bool>` và `Action` là các hàm lambda lưu trong biến.
- `Show()`:
  - Lọc các kỹ năng còn nâng được, xáo ngẫu nhiên (`OrderBy(_ => Random.value)`), lấy 3.
  - Gán chữ và sự kiện `onClick` cho 3 nút.
  - Hiện bảng, **dừng game** (`Time.timeScale = 0`).
- `Choose(upgrade)`: áp dụng kỹ năng, ẩn bảng, cho game chạy lại, báo `GameManager.OnUpgradeChosen()`.

### 2.15 `HUD.cs` – Giao diện trong game

**Gắn vào:** `Canvas`. Chỉ hiển thị, không chứa luật chơi.

| Hàm | Hiển thị |
|---|---|
| `SetScore(score)` | `SCORE: 240` (góc trên trái) |
| `SetLevel(level, progress)` | `LV 3` và thanh xanh (Image kiểu *Filled*, `fillAmount` từ 0 đến 1) |
| `SetWave(wave, isBoss, left)` | `WAVE 3 - ENEMIES: 5` hoặc `WAVE 5 - BOSS` |
| `ShowWaveBanner(text)` | Chữ lớn giữa màn hình trong 2 giây (coroutine) |
| `SetLives(lives)` | 3 trái tim góc trên phải; tim đã mất bị làm mờ (alpha 0.2) |
| `ShowBossBar` / `SetBossHealth` / `HideBossBar` | Thanh máu boss ở giữa phía trên |

### 2.16 `CameraFollow.cs` – Camera bám theo tàu

**Gắn vào:** `Main Camera`.

- `LateUpdate()` (chạy sau khi mọi thứ đã di chuyển xong trong frame, để camera không bị giật):
  - Tính nửa chiều cao (`orthographicSize`) và nửa chiều rộng (`× cam.aspect`) của khung nhìn.
  - Kẹp (`Mathf.Clamp`) vị trí camera để **không bao giờ nhìn ra ngoài map**.
  - Di chuyển mượt tới đó bằng `Vector3.Lerp`.

---

## 3. Script của Lobby và lưu dữ liệu

### 3.1 `LobbyMenu.cs` – Màn hình chờ

**Gắn vào:** `LobbyMenu` trong scene `Lobby`. Các hàm public được gắn vào sự kiện `onClick` của nút.

| Hàm | Nút | Việc làm |
|---|---|---|
| `Play()` | PLAY | `SceneManager.LoadScene("SpaceScene")` |
| `ShowMain()` | RETURN | Về màn chính, hiện `BEST SCORE` |
| `ShowTutorial()` | TUTORIAL | Hiện bảng hướng dẫn |
| `ShowHistory()` | HISTORY | Hiện lịch sử lượt chơi |

`ShowHistory()`:
- Đọc danh sách bằng `RunHistory.Load()`.
- Tìm lượt có điểm cao nhất (`runs.Max`).
- In dòng **BEST** màu vàng ở đầu, sau đó liệt kê 10 lượt gần nhất (mới nhất ở trên). Lượt có điểm cao nhất được tô vàng và có thêm chữ `★ BEST`.
- Màu chữ dùng *rich text*: `<color=#FFD54A>...</color>`.

### 3.2 `RunHistory.cs` – Lưu lịch sử bằng PlayerPrefs

- `RunRecord`: dữ liệu 1 lượt chơi (`score`, `level`, `wave`, `enemiesKilled`, `bossesKilled`, `playedAt`). Có `[Serializable]` để `JsonUtility` chuyển được sang JSON.
- `Load()`: đọc chuỗi JSON từ `PlayerPrefs` (key `RunHistory`) và chuyển ngược thành danh sách.
- `Save(record)`: ghi thời gian chơi (`DateTime.Now`), thêm vào danh sách (giữ tối đa 50 lượt), lưu lại dạng JSON. Trả về `true` nếu là điểm cao nhất.
- `BestScore()`: điểm cao nhất (0 nếu chưa chơi lần nào).

`PlayerPrefs` lưu dữ liệu nhỏ trên máy và vẫn còn sau khi tắt game. `JsonUtility` không đọc trực tiếp được một `List`, nên script bọc danh sách trong class `RunList`.

---

## 4. Công cụ Editor: `Assets/Editor/BuildLab1.cs`

Đây **không phải script trong game**. Nó là công cụ chạy trong Unity Editor, đã được dùng để tự động dựng 2 scene và 6 prefab:
- import sprite;
- tạo tag;
- tạo các GameObject, gắn component, gán tham chiếu giữa các script;
- dựng UI.

Vì nằm trong thư mục `Editor`, nó **không được đưa vào bản build game**.

> Lưu ý: chạy lại `BuildLab1.Run` sẽ **dựng lại scene từ đầu**, ghi đè mọi chỉnh sửa tay trong 2 scene. Nếu bạn muốn chỉnh scene bằng tay trong Editor thì không chạy lại công cụ này.

---

## 5. Câu hỏi giáo viên có thể hỏi

| Câu hỏi | Trả lời ngắn | Xem ở |
|---|---|---|
| Thiên thạch/sao sinh ngẫu nhiên thế nào? | `Random.Range` trong `MapBounds`, tránh xa tàu `safeDistance` | `Spawner.Spawn` |
| Di chuyển ngẫu nhiên thế nào? | `Random.insideUnitCircle.normalized * tốc độ`; sao đổi hướng mỗi 2 giây | `Asteroid.Start`, `Star` |
| Vì sao thiên thạch nảy ở biên? | Physics Material 2D `Bouncy` (bounciness 1) trên thiên thạch và tường | prefab `Asteroid`, `Map/Walls` |
| Súng bắn thế nào? | `Instantiate` đạn ở `Gun`, đạn bay theo `transform.up`, cooldown bằng `nextFireTime` | `Gun`, `Bullet` |
| Trigger khác Collision ra sao? | Trigger đi xuyên, chỉ báo `OnTriggerEnter2D` (đạn, sao); Collision va chạm vật lý thật (`OnCollisionEnter2D`) | `Bullet`, `PlayerController` |
| 3 mạng hoạt động ra sao? | `PlayerHealth.TakeHit` trừ tim, bất tử 2 giây và nhấp nháy | `PlayerHealth` |
| Lên level thế nào? | Theo tổng điểm: `40 × L × (L − 1)` | `GameManager.ScoreForLevel` |
| Wave và boss? | Wave n có `3 + 2(n−1)` địch; wave chia hết cho 5 chỉ có boss, boss mạnh dần | `WaveManager`, `Boss.Setup` |
| Best score lưu ở đâu? | `PlayerPrefs` dạng JSON | `RunHistory` |
| Tạm dừng game khi chọn kỹ năng? | `Time.timeScale = 0` | `UpgradeManager.Show` |
