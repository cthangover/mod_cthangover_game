# Cthangover: Story — trò chơi chính

Cthangover là tiểu thuyết tương tác mang màu sắc châm biếm u tối, kể về Marao — người đàn ông tỉnh dậy trong một con hẻm sau một đêm tồi tệ: mất trí nhớ, trùm lên mình mảnh vải đỏ và bên cạnh là cuốn sách cũ mà anh không nhớ mình đã mua. Mod này là nội dung riêng của trò chơi: các cảnh, kịch bản, nhân vật, vật phẩm, nhiệm vụ, bản đồ thế giới, cách chọn nhạc và văn bản câu chuyện. Mọi thứ còn lại — hội thoại, túi đồ, chiến đấu, màn hình bản đồ — do các mod engine và mod cơ chế mà trò chơi kết hợp lại đảm nhận.

Chơi từ **Chơi → Cthangover → Trò chơi mới**.

![Cảnh đầu tiên của ván mới: Marao quấn mảnh vải đỏ đứng trong con hẻm tối, lời kể tả cơn say của anh. Phần văn bản mở đầu hiện chỉ có tiếng Nga.](assets/opening.jpg)

## Trong trò chơi có gì

- **Mở đầu.** Câu chuyện bắt đầu ở con hẻm, đi qua nghĩa địa và men theo bờ sông tới một biệt thự cũ. Phần mở đầu chơi như một cuốn tiểu thuyết: từng dòng văn bản, chân dung Marao và các lựa chọn.
- **Biệt thự và thị trấn.** Sau phần mở đầu, bạn di chuyển giữa các địa điểm bằng lựa chọn: bên ngoài biệt thự, các căn phòng trong đó (tiền sảnh, phòng khách, nhà bếp, phòng ngủ, phòng làm việc, tầng hầm), lối vào thị trấn và vùng ngoại ô.
- **Chi tiết sinh tồn.** Ván mới bắt đầu với thịt sói và củi trong túi; nhà bếp theo dõi lượng lương thực và mức tiêu hao mỗi ngày, đồng thời cho phép nấu nướng.
- **Nhiệm vụ.** Nhật ký theo dõi các việc của câu chuyện, bắt đầu với *Thức tỉnh* và *Ngôi nhà cũ*.
- **Bản đồ thế giới.** Bảng bản đồ cho thấy khu vực cùng những nơi bạn có thể tới.
- **Chiến đấu.** Các trận đánh mặc định dùng chiến đấu thẻ bài; có thể chuyển sang kiểu chiến đấu ra lệnh trong phần cài đặt của trò chơi.

![Ngoài biệt thự lúc giữa trưa: ngôi nhà cối xay bên thác nước với các lựa chọn "Vào nhà", "Vào thị trấn" và "Vào rừng".](assets/villa-outside.jpg)

![Nhà bếp: bộ đếm lương thực ở góc trên trái và các lựa chọn "Rời đi" và "Nấu ăn".](assets/kitchen.jpg)

![Bản đồ thế giới của khu phía bắc với các mốc Biệt thự và Lối vào thị trấn.](assets/world-map.jpg)

![Một trận chiến thẻ bài trong thị trấn: Marao và một người đồng hành đấu với hai con sói.](assets/battle.jpg)

## Ngôn ngữ

Kịch bản phần mở đầu viết bằng tiếng Nga và chưa có bản dịch, nên văn bản luôn hiện bằng tiếng Nga dù giao diện dùng ngôn ngữ nào. Các cảnh biệt thự và thị trấn, nhật ký nhiệm vụ, vật phẩm và trình đơn đã được dịch.

## Trò chơi được ghép như thế nào

Trò chơi là bộ sưu tập **Cthangover**. Các khối của nó cho biết mod nào đảm nhận việc gì:

```text
Cốt truyện     cthangover_game, cthangover_murakami, cthangover_party, town_pack
Engine 2D      shell, hội thoại, tương tác, thời gian và ánh sáng, hiệu ứng, bản đồ thế giới
Nhân vật       nhân vật và kỹ năng
Nhiệm vụ       nhật ký
Vật phẩm       túi đồ, nấu ăn
Chiến đấu      nền tảng theo lượt + chiến đấu thẻ bài (lõi mặc định)
Chiến đấu (FF) lõi chiến đấu kiểu ra lệnh (tùy chọn)
Tuyển mộ       tuyển mộ nhân vật đã bị đánh bại (tùy chọn)
Live2D         chân dung động (tùy chọn)
Giao diện      bộ công cụ UI, trình đơn, âm thanh
Công cụ phát triển (tùy chọn)  trình soạn thảo, trình kiểm tra, atlas, trình quản lý mod
```

- **Cthangover: Murakami** thêm một tuyến truyện riêng, còn **Cthangover: Starter Party** đưa Marao vào đội ngay từ đầu. Cả hai thuộc cùng một trò chơi nhưng là những mod riêng biệt.
- **Town Pack** cung cấp hình lối vào thị trấn và tiền sảnh; **Music Pack** cung cấp nhạc nền. Mod này thêm một danh sách phát riêng cho cảnh lối vào thị trấn.
- Bộ sưu tập đặt `battle.core` thành `card_battle`. Bạn có thể đổi giá trị này, cũng như các mod của bất kỳ khối nào, trong **Mods & Collections → Bộ sưu tập → Cthangover → Sửa sơ đồ**; các bản lưu của bạn vẫn được giữ nguyên.

## Dành cho tác giả

Nội dung nằm trong những thư mục bình thường:

```filestree
cthangover_game/
├── scenes/          mỗi địa điểm một JSON: tên, nền, âm thanh nền, kịch bản mặc định
├── scenarios/       prolog/, home/, town/ và start.scenario
├── characters/      người chơi, kẻ địch, kỹ năng
├── items/           đồ ăn, công thức, tài nguyên
├── quests/          quests.json
├── interactives/    vật thể bấm được trong các cảnh mở đầu
├── map/map.json     vùng và mốc của bản đồ thế giới
├── backgrounds/, avatars/, sounds/, effects/, shaders/
└── locale/          tên, nhiệm vụ và bản dịch kịch bản bằng bảy ngôn ngữ
```

Ván mới bắt đầu ở `start_scene`, kịch bản của nó phát vật phẩm khởi đầu rồi chuyển sang con hẻm:

```scenario
scene: start_scene
priority: 0
is_one_run: true
---
action item.add id=food/wolf_meat count=10
action item.add id=resource/branches count=10
switch_scene prolog_alley
end
```

Muốn đổi văn bản của một cảnh mà không sửa mod này, hãy thêm một kịch bản cho chính cảnh đó vào mod của bạn, hoặc thay thế tệp bằng cách đóng gói tệp trùng tên cùng phần phụ thuộc vào `cthangover_game`.

## Nếu có trục trặc

1. **Thẻ trò chơi báo chưa sẵn sàng.** Dùng **Cài phần còn thiếu** trong **Chơi**, hoặc kiểm tra lại sơ đồ.
2. **Không vào được trận chiến.** Lõi chiến đấu đã chọn phải nằm trong trò chơi.
3. **Văn bản câu chuyện bằng tiếng Nga.** Đó là phần mở đầu (xem Ngôn ngữ).
