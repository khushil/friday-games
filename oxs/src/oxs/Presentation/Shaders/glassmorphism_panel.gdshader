shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, filter_linear_mipmap;
uniform vec4 tint_color : source_color = vec4(1.0, 1.0, 1.0, 0.15);
uniform vec4 border_color : source_color = vec4(1.0, 1.0, 1.0, 0.3);
uniform float blur_amount : hint_range(0.0, 5.0) = 2.0;
uniform float border_width : hint_range(0.0, 10.0) = 1.5;
uniform float corner_radius : hint_range(0.0, 50.0) = 16.0;

// Rounded rectangle SDF (Signed Distance Function)
float rounded_rect_sdf(vec2 p, vec2 half_size, float radius) {
    vec2 q = abs(p) - half_size + radius;
    return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - radius;
}

void fragment() {
    vec2 pixel_size = 1.0 / vec2(textureSize(screen_texture, 0));
    vec2 rect_size = 1.0 / TEXTURE_PIXEL_SIZE;
    vec2 pixel_pos = UV * rect_size;
    vec2 center = rect_size * 0.5;

    // Calculate SDF for rounded rectangle
    float dist = rounded_rect_sdf(pixel_pos - center, center, corner_radius);

    // Sample blurred background using LOD
    vec4 blurred = textureLod(screen_texture, SCREEN_UV, blur_amount);

    // Apply glass tint
    vec4 glass_color = mix(blurred, tint_color, tint_color.a);

    // Edge softness for anti-aliasing
    float edge_softness = 1.5;
    float shape_alpha = 1.0 - smoothstep(-edge_softness, edge_softness, dist);

    // Border calculation
    float border_alpha = 1.0 - smoothstep(0.0, border_width, abs(dist));

    // Combine glass and border
    vec4 final_color = glass_color;
    final_color = mix(final_color, border_color, border_alpha * border_color.a);
    final_color.a *= shape_alpha;

    // Add subtle highlight at top edge (glassmorphism characteristic)
    float highlight = smoothstep(0.0, 0.15, UV.y) * (1.0 - UV.y) * 0.2;
    final_color.rgb += vec3(highlight);

    COLOR = final_color;
}
