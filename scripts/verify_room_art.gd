extends SceneTree
## Run in a minimal Godot 4.5.1 project: --script <this file> -- --pck=<built PCK> --preview=<optional PNG>
var preview_path = ""
func _initialize():
    call_deferred("verify")

func verify():
    var packed = false
    for argument in OS.get_cmdline_user_args():
        if argument.begins_with("--pck="):
            assert(ProjectSettings.load_resource_pack(argument.trim_prefix("--pck=")))
            packed = true
        if argument.begins_with("--preview="):
            preview_path = argument.trim_prefix("--preview=")
    assert(packed, "Pass a built PCK to verify the shipped texture remaps")
    var viewport = SubViewport.new()
    viewport.size = Vector2i(1000, 720)
    viewport.render_target_update_mode = SubViewport.UPDATE_ALWAYS
    root.add_child(viewport)
    for rest in [true, false]:
        var stem = "hero_rest_site" if rest else "hero_shop"
        var texture = load("res://IsekaiHero/images/character/%s_v1.png" % stem) as Texture2D
        assert(texture != null, "Packaged PNG import remap must load")
        var image = texture.get_image()
        assert(image != null and image.get_format() == Image.FORMAT_RGBA8)
        assert(image.get_pixel(0, 0).a == 0 and image.get_pixel(image.get_width()-1, 0).a == 0)
        assert(image.get_pixel(image.get_width()/2, image.get_height()/2).a > 0.95)
        var background = ColorRect.new()
        background.position = Vector2(0 if rest else 500, 0)
        background.size = Vector2(500, 720)
        background.color = Color("263124" if rest else "402a30")
        viewport.add_child(background)
        var sprite = Sprite2D.new()
        sprite.texture = texture
        sprite.centered = false
        sprite.flip_h = not rest
        # Same source scale and origin as production, including camp's inherited 0.76 scale.
        var factor = 0.56 * 0.760006 if rest else 0.30
        sprite.scale = Vector2.ONE * factor
        sprite.position = Vector2(260 - texture.get_width()*factor/2, 350 - 364*0.760006) if rest else Vector2(750 - texture.get_width()*factor/2, 610 - 453)
        viewport.add_child(sprite)
        var label = Label.new()
        label.text = "Rest site" if rest else "Shop"
        label.position = Vector2(30 if rest else 530, 25)
        label.add_theme_font_size_override("font_size", 24)
        viewport.add_child(label)
        print("PASS packaged ", stem, ": ", image.get_size(), ", alpha and texture remap")
    if not preview_path.is_empty():
        await process_frame
        await RenderingServer.frame_post_draw
        assert(viewport.get_texture().get_image().save_png(preview_path) == OK)
        print("Saved native Godot preview: ", preview_path)
    viewport.queue_free()
    quit(0)
