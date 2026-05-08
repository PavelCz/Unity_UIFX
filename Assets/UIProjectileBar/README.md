# UI Projectile Bar Effect

This guide explains how to move the projectile-driven UI bar effect into another Unity project and how to tune the adjustable values in the Inspector.

The effect is built from standard uGUI, a world-space `TrailRenderer`/`ParticleSystem` projectile, an animated `Slider`, and optional impact glow on target icons.

## Requirements

- Unity with uGUI.
- URP is recommended.
- Bloom is recommended if you want HDR colors to visibly glow.
- The Canvas should render through a camera, not pure overlay, because the projectile uses normal Unity renderers.

## Assets To Copy

Copy these files into your project:

- `EnergyProjectile.prefab`
- `M_ProjectileTrail.mat`
- `EnergyBarMat.mat`
- `HDRBorderMat.mat`
- `EnergyBarShader.shadergraph`
- `UI_HDR_Border.shadergraph`
- `ItemButton.cs`
- `UIProjectile.cs`
- `EnergySliderAnimator.cs`
- `IconImpactGlow.cs`

Also copy any target/icon sprites you want to use, such as `char.png`.

## Integration Guide

### 1. Camera And Canvas

1. Create or select your main camera.
2. Set the camera clear mode/background as desired.
3. Create a Canvas.
4. Set the Canvas **Render Mode** to `Screen Space - Camera`.
5. Assign the main camera to the Canvas **Render Camera** field.
6. Use a low Canvas **Plane Distance**, such as `1`.

The projectile is not a Canvas element. It uses `TrailRenderer` and `ParticleSystemRenderer`, so it must be visible to the camera.

### 2. Add The Slider

1. Create a UI `Slider`.
2. Set `Min Value` to `0`.
3. Set `Max Value` to `1`.
4. Set the starting `Value` to `0`.
5. Assign your fill image/material. In this project the fill uses `EnergyBarMat`.
6. Add `EnergySliderAnimator` to the Slider object.

The `EnergySliderAnimator` component makes value changes slide smoothly and creates the moving white/cyan edge effect while the bar increases.

### 3. Add A Projectile Root

Create an empty GameObject in the scene named something like `VFXRoot`.

Use this as the `projectileRoot` for buttons. Keeping projectiles under a world-space root is cleaner than parenting them to the Canvas. The projectiles still use normal camera rendering.

### 4. Add Buttons

For each UI button:

1. Add the `ItemButton` component.
2. Assign `borderImage` to the button's border image.
3. Assign `buttonComponent` to the button's `Button` component.
4. Assign `projectilePrefab` to `EnergyProjectile.prefab` if this button fires a projectile.
5. Assign `destinationTarget` to the target transform.
6. Assign `projectileRoot` to `VFXRoot`.
7. Assign `targetSlider` if this button should change the bar.
8. Set `sliderIncrement` to the amount this button adds.

For a reset button, leave the projectile fields empty, assign `targetSlider`, and enable `resetSliderOnClick`.

### 5. Add Target Impact Glow

For target icons that should flash when hit:

1. Select the target UI Image.
2. Add `IconImpactGlow`.
3. Leave `targetImage` empty unless you want to reference a different image.
4. Tune the fallback color and fade values if needed.

Any projectile button whose `destinationTarget` points to this object, or a child of this object, will trigger the glow on impact.

### 6. Renderer Sorting

If projectile particles render behind UI:

- Set the `TrailRenderer` sorting order higher than the Canvas, for example `100`.
- Set the `ParticleSystemRenderer` sorting order slightly higher, for example `101`.
- Confirm the camera culling mask includes the projectile layer.

## Usage And Tuning Guide

### ItemButton

Controls how a UI button behaves when clicked.

- `borderImage`: Image whose material glow is animated on click.
- `buttonComponent`: The Unity UI Button to listen to.
- `projectilePrefab`: Projectile prefab fired by this button. Leave empty for non-projectile buttons such as reset.
- `destinationTarget`: Transform the projectile flies toward.
- `projectileRoot`: Scene parent for spawned projectile instances, usually `VFXRoot`.
- `targetSlider`: Slider affected when the projectile hits, or reset immediately if this is a reset button.
- `sliderIncrement`: Amount added to the slider on hit. With a `0..1` slider, `0.1` means 10%.
- `resetSliderOnClick`: If enabled, clicking this button animates the target slider down to its minimum.
- `activeGlowColor`: HDR color used for button border flash, projectile trail/particles, and target impact glow.
- `inactiveColor`: Resting border glow color after fade-out.
- `fadeInDuration`: Time for the button border to brighten.
- `fadeDuration`: Time for the button border to fade back.

### UIProjectile

Controls the moving projectile.

- `trail`: The projectile trail renderer.
- `sparks`: Particle burst played when the projectile reaches the target.
- `speed`: How quickly the projectile moves along the curve.
- `arcHeight`: Base height of the Bezier arc.
- `arcHeightRandomness`: Per-shot random arc height variation. Increase for more varied vertical curves.
- `sideOffsetRandomness`: Per-shot sideways control-point variation. Increase for more varied paths.

Useful tuning notes:

- Reduce `TrailRenderer > Width Multiplier` if the trail is too thick.
- Reduce `Sparks > Main > Start Size` if the impact particles are too large.
- Reduce `Sparks > Main > Start Speed` or `Start Lifetime` if the burst spreads too far.

### EnergySliderAnimator

Controls the smooth bar animation and the bright leading-edge effect.

- `slider`: Slider to animate. Leave empty to auto-use the Slider on the same object.
- `slideDuration`: Time for the bar to move to the new value.
- `slideCurve`: Easing curve for both increases and resets.
- `edgeCoreColor`: Color of the narrow bright core at the fill edge.
- `edgeGlowColor`: Color of the softer glow around the fill edge.
- `edgeCoreSize`: Width/height of the bright core.
- `edgeGlowSize`: Width/height of the soft glow.
- `sparkCount`: Number of small spark images around the fill edge.
- `sparkSizeRange`: Min/max spark size.
- `sparkSpread`: Horizontal/vertical spread of the sparks.

Increases show the white/cyan leading-edge effect. Resets slide down without the bright edge flare.

### IconImpactGlow

Controls the sprite-shaped glow when a projectile hits a target icon.

- `targetImage`: Image used as the glow mask/source. Leave empty to use the Image on the same object.
- `fallbackGlowColor`: Color used if `Play()` is called without a color.
- `fadeInDuration`: How fast the icon glow appears.
- `fadeOutDuration`: How long the glow fades away.
- `punchScale`: Temporary scale applied to the target icon on impact.

The component duplicates the target sprite behind the icon, so the flash follows the sprite alpha shape.

### Materials And Visual Quality

- `EnergyBarMat`: Material for the slider fill.
- `HDRBorderMat`: Material used by button borders for click glow.
- `M_ProjectileTrail`: Material used by the trail and impact particles.

For stronger glow, enable Bloom in your URP Global Volume and use HDR colors with values above `1`, such as red `(4, 0, 0, 1)` or blue `(0, 0, 4, 1)`.

On mobile, Bloom can be one of the more expensive parts of this effect. Test on target devices, keep Bloom intensity modest, and consider a no-Bloom fallback that relies on the built-in UI glow sprites, trail color, and impact particles.

## Troubleshooting

- UI appears but projectiles do not: make sure the Canvas uses `Screen Space - Camera` and the camera sees the projectile layer.
- Projectile appears behind UI: raise `TrailRenderer` and `ParticleSystemRenderer` sorting orders.
- Projectile or particles are invisible: check that `activeGlowColor.a` is not `0`.
- Trail is too wide: lower `TrailRenderer > Width Multiplier`.
- Particles are too large: lower `ParticleSystem > Main > Start Size`.
- Slider jumps instead of sliding: make sure `EnergySliderAnimator` is attached to the Slider.
- Target icon does not glow: add `IconImpactGlow` to the target Image, and make sure the projectile button's `destinationTarget` points to that target or one of its children.
- Sprite looks stretched: enable `Preserve Aspect` on the target Image.
