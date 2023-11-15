shader_type canvas_item;

// Input grayscale texture
uniform sampler2D grayscale_texture : hint_normal;

uniform float threshold : hint_range(0.0, 1.0) = 0.5;

void vertex() {
    // No need for any vertex manipulation in this shader, so we leave this empty.
}

void fragment() {
    // Get the color of the current pixel from the grayscale texture.
    vec4 grayscale_color = textureLod(grayscale_texture, UV, 0.0);

    // Use the red channel of the grayscale texture as the grayscale value.
    float grayscale = grayscale_color.r;

    // If the grayscale value is above the threshold, set the pixel to the original color.
    // Otherwise, set it to transparent (alpha = 0).
    if (grayscale < threshold) {
        // Get the color of the current pixel from the canvas item texture.
        vec4 color = textureLod(TEXTURE, UV, 0.0);
        COLOR = color;
    } else {
        COLOR = vec4(0.0, 0.0, 0.0, 0.0);
    }
}
