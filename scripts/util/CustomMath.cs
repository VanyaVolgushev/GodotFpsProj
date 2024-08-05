using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;

public static class CustomMath
{
    public static Vector3 SlideAlongMultiple(Vector3 translation, Vector3 initialTranslation, List<Vector3> collisionNormals, float ParallelismTolerance, float TranslationCutoff)
	{
		if(collisionNormals.Count == 0)
		{
			return translation;
		}
		else if(collisionNormals.Count == 1)
		{
			if(collisionNormals[0].Dot(translation) <= 0)
			{
				return MoveAndSlide(translation, collisionNormals[0], 0).projectedLeftOver;
			}
			else
			{
				return translation;
			}
		}
        else if (collisionNormals.Count == 2)
        {
            Vector3 creaseVector = collisionNormals[0].Cross(collisionNormals[1]);
            if (creaseVector != Vector3.Zero)
            {
                creaseVector = creaseVector.Normalized();
            }
			Vector3 innerCreasePlane0 = collisionNormals[0].Cross(creaseVector).Normalized();
			Vector3 innerCreasePlane1 = creaseVector.Cross(collisionNormals[1]).Normalized();
			bool rightSideOfCreasePlane0 = innerCreasePlane0.Dot(initialTranslation) > 0;
			bool rightSideOfCreasePlane1 = innerCreasePlane1.Dot(initialTranslation) > 0;
			if(rightSideOfCreasePlane0 && rightSideOfCreasePlane1)
			{
				//moving into crease
            	float projectionMagnitude = initialTranslation.Dot(creaseVector);
            	Vector3 projectedLeftOver = creaseVector * projectionMagnitude;
            	return projectedLeftOver;
			}
			else if(!rightSideOfCreasePlane0 && !rightSideOfCreasePlane1)
			{
				return translation;
			}
			else if(!rightSideOfCreasePlane0)
			{
				return MoveAndSlide(initialTranslation.Normalized() * translation.Length(), collisionNormals[0], 0).projectedLeftOver;
			}
			else if(!rightSideOfCreasePlane1)
			{
				return MoveAndSlide(initialTranslation.Normalized() * translation.Length(), collisionNormals[1], 0).projectedLeftOver;
			}
			else
			{
				GD.Print("ERROR: moving neither outside nor inside crease");
				return Vector3.Zero;
			}
		}
		else
		{
			
		}
		return Vector3.Zero;
		
		/*
		float totalDotSum = 0;
		if(collisionNormals.Count == 0)
		{
			throw new Exception("normal list was empty");
		}
		foreach(Vector3 normal in collisionNormals)
		{
			totalDotSum += normal.Dot(-translation.Normalized());
			GD.Print(new Vector2(1,-1).Project(new Vector2(0,1)));
		}
		foreach(Vector3 normal in collisionNormals)
		{
			Vector3 counterTranslation = -translation.Project(normal);
			float coeff = counterTranslation.Normalized().Dot(-translation.Normalized());
			translation += counterTranslation * coeff  / totalDotSum;
		}
		return translation;
		*/
	}
	public static (Vector3 toSurface, Vector3 projectedLeftOver) MoveAndSlide(Vector3 translation, Vector3 collisionNormal, float collisionSafeFraction)
	{
		Vector3 leftOverTranslation = translation * (1 - collisionSafeFraction);
		Vector3 translationToSurface = translation * collisionSafeFraction;
		Vector3 projectedLeftOverTranslation = leftOverTranslation - leftOverTranslation.Dot(collisionNormal) * collisionNormal;
		return (translationToSurface, projectedLeftOverTranslation);
	}
}