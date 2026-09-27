extends SceneTree
## Run with Godot 4.5.1 --headless --path <repo> --script scripts/verify_character_animations.gd

func _initialize():
	call_deferred("verify")

func verify():
	var packed = false
	for argument in OS.get_cmdline_user_args():
		if argument.begins_with("--pck="):
			assert(ProjectSettings.load_resource_pack(argument.trim_prefix("--pck=")))
			packed = true
	var library = load("res://IsekaiHero/animations/hero_combat.tres") as AnimationLibrary
	assert(library != null, "Animation library must load in Godot")
	assert(library.get_animation_list().size() == 7)
	var texture: Texture2D
	if packed:
		texture = load("res://IsekaiHero/images/character/hero_combat_atlas.png")
	else:
		texture = ImageTexture.create_from_image(Image.load_from_file("res://IsekaiHero/images/character/hero_combat_atlas.png"))
	assert(texture != null, "The texture import remap must resolve")
	var image = texture.get_image()
	assert(image != null and image.get_size() == Vector2i(1536, 1024))
	assert(image.get_pixel(0, 0).a == 0 and image.get_pixel(512, 512).a == 0,
		"Empty atlas space must be transparent")
	var visual_root = Node2D.new()
	var body = Node2D.new()
	body.name = "Visuals"
	visual_root.add_child(body)
	var sprite = Sprite2D.new()
	sprite.name = "Sprite"
	sprite.texture = texture
	sprite.centered = false
	sprite.region_enabled = true
	sprite.region_filter_clip_enabled = true
	sprite.scale = Vector2(0.65, 0.65)
	body.add_child(sprite)
	var player = AnimationPlayer.new()
	visual_root.add_child(player)
	player.add_animation_library("", library)
	for action in ["Attack", "Cast", "Hit", "Revive"]:
		player.animation_set_next(action, "Idle")
	root.add_child(visual_root)
	player.callback_mode_process = AnimationMixer.ANIMATION_CALLBACK_MODE_PROCESS_MANUAL
	for animation_name in library.get_animation_list():
		var animation = library.get_animation(animation_name)
		assert(animation.get_track_count() == 5)
		for track in range(animation.get_track_count()):
			assert(visual_root.has_node(NodePath(str(animation.track_get_path(track)).split(":")[0])))
		player.play(animation_name)
		player.advance(0)
		for frame in range(int(ceil(animation.length * 60))):
			player.advance(1.0 / 60)
			assert(sprite.region_rect.size.x > 0 and sprite.region_rect.size.y == 512)
			assert(Rect2(0, 0, 1536, 1024).encloses(sprite.region_rect))
			assert(body.scale.x > 0 and body.scale.y > 0)
		player.advance(0.05)
		if animation_name in ["Attack", "Cast", "Hit", "Revive"]:
			assert(player.current_animation == "Idle", "One-shot must return to idle")
		if animation_name == "Dead":
			assert(not player.is_playing(), "Death must hold its final pose")
			assert(sprite.region_rect.position == Vector2(1024, 512))
	# Simulate attacks being interrupted by damage, death, then revival.
	for animation_name in ["Attack", "Hit", "Cast", "Dead", "Revive", "Idle"]:
		player.play(animation_name)
		player.advance(0.1)
	player.play("Idle")
	player.advance(0)
	assert(body.position == Vector2.ZERO)
	assert(sprite.offset == Vector2(-285, -480))
	assert(sprite.region_rect == Rect2(0, 0, 512, 512))
	print("PASS: seven native animations, atlas alpha, all track paths, regions, action-to-idle transitions, death hold and interruption reset")
	visual_root.queue_free()
	quit(0)
