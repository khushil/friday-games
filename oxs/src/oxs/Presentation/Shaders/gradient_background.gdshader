shader_type canvas_item;

uniform vec4 color_top : source_color = vec4(0.4, 0.494, 0.918, 1.0);
uniform vec4 color_bottom : source_color = vec4(0.463, 0.294, 0.635, 1.0);
uniform float noise_intensity : hint_range(0.0, 0.1) = 0.02;
uniform float time_scale : hint_range(0.0, 1.0) = 0.1;

// Simple noise function for subtle texture
float hash(vec2 p) {
    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453);
}

void fragment() {
    // Animated gradient with subtle wave movement
    float wave = sin(TIME * time_scale + UV.x * 2.0) * 0.02;
    float t = UV.y + wave;

    // Smooth gradient interpolation
    vec4 gradient = mix(color_top, color_bottom, smoothstep(0.0, 1.0, t));

    // Add subtle noise for texture (prevents banding)
    float noise = (hash(UV * 100.0 + TIME * 0.01) - 0.5) * noise_intensity;
    gradient.rgb += noise;

    COLOR = gradient;
}
