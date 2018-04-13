using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrushstrokeMapGen : MapGenBase {
	public float minAcceleration;
	public float maxAcceleration;
	public float minDamping;
	public float maxDamping;
	public float minimumRadius;
	public float maximumRadius;
	public int stepCount;
	[Range(0f,1f)]
	public float bounce;
	public int ticksPerRadiusChange;
	public int stepsPerFrame;

	float acceleration;
	float damping;
	float minRadius;
	float maxRadius;

	Vector2 position;
	Vector2 velocity;
	float radius;
	int stepsRemaining;

	float radiusTimer;
	float animEndRadius;
	float animStartRadius;

	Vector2 startPosition;

	void AnimateRadius() {
		animStartRadius = radius;
		animEndRadius = Random.Range(minRadius,maxRadius);
		radiusTimer = 0f;
	}

	public override void StartGenerator() {
		if (fastForward) {
			stepsPerFrame = stepCount;
		}
		minRadius = Random.Range(minimumRadius,maximumRadius);
		maxRadius = Random.Range(minimumRadius,maximumRadius);
		acceleration = Random.Range(minAcceleration,maxAcceleration);
		damping = Random.Range(minDamping,maxDamping);
		if (minRadius>maxRadius) {
			float temp = minRadius;
			minRadius = maxRadius;
			maxRadius = temp;
		}

		radius = Random.Range(minRadius,maxRadius);
		AnimateRadius();
		position = new Vector2(Random.value,Random.value);
		startPosition = position;
		velocity = Vector2.zero;
		stepsRemaining = stepCount;

		map.FillMap(TileType.Wall);
	}

	public override void TickGenerator() {
		if (stepsRemaining > 0) {
			int loopCount = Mathf.Min(stepsPerFrame,stepsRemaining);
			for (int i = 0; i < loopCount; i++) {
				stepsRemaining--;
				radiusTimer += 1f / ticksPerRadiusChange;
				radius = Mathf.SmoothStep(animStartRadius,animEndRadius,radiusTimer);
				if (radiusTimer > 1f) {
					AnimateRadius();
				}

				velocity += Random.insideUnitCircle * acceleration;
				velocity *= damping;
				float steps = velocity.magnitude;
				while (steps > 0f) {
					float stepDist = 1f;
					if (stepDist<steps) {
						stepDist = steps;
						steps = 0f;
					} else {
						steps -= 1f;
					}
					position += velocity.normalized*stepDist/map.width;
					if (position.x < 0.02f) {
						position.x = 0.02f;
						velocity.x *= -bounce;
					}
					if (position.x > .98f) {
						position.x = .98f;
						velocity.x *= -bounce;
					}
					if (position.y < 0.02f) {
						position.y = 0.02f;
						velocity.y *= -bounce;
					}
					if (position.y > .98f) {
						position.y = .98f;
						velocity.y *= -bounce;
					}

					Vector2 tilePos = new Vector2(position.x * map.width,position.y * map.height);
					float sqrRadius = radius * radius;
					for (int x = (int)(tilePos.x - radius); x <= (int)(tilePos.x + radius + 1f); x++) {
						for (int y = (int)(tilePos.y - radius); y <= (int)(tilePos.y + radius + 1f); y++) {
							float dx = x + .5f - tilePos.x;
							float dy = y + .5f - tilePos.y;
							float sqrDist = dx * dx + dy * dy;
							if (sqrDist < sqrRadius) {
								map.SetTile(x,y,TileType.Open);
							}
						}
					}
				}
			}
			map.ApplyTex();
		}

		if (stepsRemaining==0 && finished==false) {
			map.SetTile((int)(startPosition.x * map.width),(int)(startPosition.y * map.height),TileType.Start);
			map.SetTile((int)(position.x * map.width),(int)(position.y * map.height),TileType.Finish);
			map.ApplyTex();
			finished = true;
		}
	}
}
