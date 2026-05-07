# CRT UI Shader Documentation

![][readme-assets/image1]

# Setup

The shader can be applied in two ways.

1. Opening the Unity project will provide a scene where the shader is already applied.  
2. Optionally, you can simply add the shader to your existing Unity project.

## Accessing the CRT Shader Using the Provided Unity Setup

1. Open the Unity project (it was created with Unity 6.4)  
2. In the Assets view, click on the CRT Shader material. The sliders and checkboxes on the right allow adjusting various properties of the shader  
   ![][readme-assets/image2]

## Adding the CRT Shader to Your Own Unity Project

The CRT Shader can be applied to uGUI images.

1. Import the image you want to apply the shader to as an asset  
2. Set Texture Type to *Spite* and Sprite Mode to *Single* using the dropdowns  
   ![][readme-assets/image3]  
3. Import the shader code. It will be a script (.shader extension).  
   ![][readme-assets/image4]  
4. Create a new material.  
   ![][readme-assets/image5]  
5. Set the material’s shader script to the CRT Shader using the dropdown.  
   ![][readme-assets/image6]  
6. In you Scene view create a Unity GameObject for your image. To do that, in the Scene Hierarchy got to New \-\> UI (Canvas) \-\> Image  
7. Apply the sprite asset you created in step 2 as the Source Image for your Image UI Object.  
   ![][readme-assets/image7]  
8. Assign the material you created in Step 4 as a material to the Image  
9. Now the shader is applied to your image. You can modify the shader properties by changing the code of the underlying script or adjusting the checkboxes and sliders when clicking on the material.

# Explanation of the Shader Properties

Various shader appearance properties can be tweaked. These can be adjusted in the shader code or when clicking on the material and adjusting the following checkboxes and sliders:  
![][readme-assets/image8]  
The other properties are default Unity shader properties that are not unique to this CRT effect.

The CRT shader consists of three main components. Each of these components can be disabled by unchecking the respective checkbox.

Additionally, there is the Image Scale setting, which is explained at the end.

## Offset Glitch Effect

The offset glitch randomly offsets lines in the image.

* **Offset Glitch Intensity:** Determines approximately how many lines will be affected simultaneously.  
* **Offset Amount (Shift):** Magnitude of the displacement of the lines.  
* **Offset Chance:** Probability of offset. Higher values mean the displacement is applied more frequently  
* **Offset Line Number:** How many lines the image is divided into. More lines means each line is narrower.

## RGB Split Effect

A chromatic aberration-type effect that offsets the color channels of the image.

* **RGB Split Amount:** Size of the displacement of the color channels  
* **RGB Split Chance:** Increases the frequency of color split.

## Scanline Effect

CRT Scanline visuals.

* **Scanline Count:** Number of scanlines.  
* **Scanline Intensity:** Higher numbers make the scanlines more visible by making them darker.

## Explanation of the Image Scale Setting

Shaders such as the Offset Effect might make part of the image protrude beyond the original confines of the image. However, shaders are limited to drawing only within the image. To get around this, the image can be scaled, which basically makes the visuals occupy less space within the original image confines, by padding at the sides. A larger scale means more padding and thus a smaller visual image. This allows the offset effects to be drawn without being cut off at the edges. To compensate for the smaller scale, the entire image can be scaled up accordingly.

