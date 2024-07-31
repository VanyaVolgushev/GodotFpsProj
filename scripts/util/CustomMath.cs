using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;

public static class CustomMath
{
    public static Vector3 SlideAlongMultiple(Vector3 translation, List<Vector3> collisionNormals, float ParallelismTolerance)
	{
		
		if(collisionNormals.Count == 0)
		{
			throw new Exception("normal list was empty");
		}
		else if(collisionNormals.Count == 1)
		{
			return MoveAndSlide(translation, collisionNormals[0], 0).projectedLeftOver;
		}
        else if (collisionNormals.Count == 2)
        {
			var collisionNormalsCopy = collisionNormals.ToList();
            Vector3 creaseVector = collisionNormalsCopy[0].Cross(collisionNormalsCopy[1]);
            if (creaseVector != Vector3.Zero)
            {
                creaseVector = creaseVector.Normalized();
            }
			Vector3 innerCreasePlane0 = collisionNormalsCopy[0].Cross(creaseVector).Normalized();
			Vector3 innerCreasePlane1 = creaseVector.Cross(collisionNormalsCopy[1]).Normalized();
			bool rightSideOfCreasePlane0 = innerCreasePlane0.Dot(translation) > 0;
			bool rightSideOfCreasePlane1 = innerCreasePlane1.Dot(translation) > 0;
			if(rightSideOfCreasePlane0 && rightSideOfCreasePlane1)
			{
				//moving into crease
            	float projectionMagnitude = translation.Dot(creaseVector);
            	Vector3 projectedLeftOver = creaseVector * projectionMagnitude;
            	return projectedLeftOver;
			}
			else if(!rightSideOfCreasePlane0)
			{
				return MoveAndSlide(translation, collisionNormals[0], 0).projectedLeftOver;
			}
			else if(!rightSideOfCreasePlane1)
			{
				return MoveAndSlide(translation, collisionNormals[1], 0).projectedLeftOver;
			}
			else
			{
				GD.Print("ERROR: moving neither outside nor inside crease");
				return Vector3.Zero;
			}
        }
		else
		{
			return Vector3.Zero;
		}
	}
	public static (Vector3 toSurface, Vector3 projectedLeftOver) MoveAndSlide(Vector3 translation, Vector3 collisionNormal, float collisionSafeFraction)
	{
		Vector3 leftOverTranslation = translation * (1 - collisionSafeFraction);
		Vector3 translationToSurface = translation * collisionSafeFraction;
		Vector3 projectedLeftOverTranslation = leftOverTranslation - leftOverTranslation.Dot(collisionNormal) * collisionNormal;
		return (translationToSurface, projectedLeftOverTranslation);
	}
}