using UnityEngine;
using static UnityEngine.Mathf;

public static class PhysicsMoveTowards
{
    public static float Square(float x) => x * x;

    public static float Velocity(float initialVelocity, float acceleration, float time)
    {
        // Kinematic formula for velocity over time
        return initialVelocity + acceleration * time;
    }

    public static float Displacement(float position, float velocity, float acceleration, float time)
    {
        // Kinematic formula for displacement over time
        return position + velocity * time + 0.5f * acceleration * Square(time);
    }

    public static float Acceleration(float distance, float time)
    {
        // Calculate acceleration required to cover distance over time, assuming we accelerate for half, and decelerate for half.
        return 4 * distance / Square(time);
    }

    /// <summary>
    /// Moves a value with a current velocity towards a target with a fixed acceleration/deceleration. Will not overshoot stationary targets, and can be used to reach moving targets, however if this 
    /// causes a situation where the acceleration limit would be exeeded, it will overshoot the target before it comes to a complete stop, after which it will then accelerate back towards the target, and decelerate to land on it.
    /// For the math, see https://www.desmos.com/calculator/iseamdozqg
    /// </summary>
    /// <param name="current">The current value</param>
    /// <param name="target">The target value, may change inbetween calls</param>
    /// <param name="velocity">The current velocity, must be stored/re-used inbetween calls, but could result in overshoot</param>
    /// <param name="acceleration">The max acceleration value. may also change if desired, but could result in overshoot</param>
    /// <param name="deltaTime">The timestep at which to advance the position and velocity</param>
    /// <returns></returns>
    public static float MoveTowards(float current, float target, ref float velocity, float acceleration, float deltaTime)
    {
        // Detect if we're in an overshoot state by checking if velocity is greater than sqrt(2 * acceleration * distance), which is the min distance to slow down, and flip some values accordingly
        var c = target < current ? -Sqrt(2 * acceleration * (current - target)) : Sqrt(2 * acceleration * (target - current));
        var a1 = velocity < c ? acceleration : -acceleration;
        var vs = velocity < c ? velocity : -velocity;

        // Calculate the halfway time. Max(0) required since precision issues may cause numbers to go very slightly negative
        var sqrtDiscriminant = Sqrt(Max(0, 0.5f * Square(velocity) + a1 * (target - current)));
        var halfwayTime = (sqrtDiscriminant - vs) / acceleration;

        // If we are at less than halfway time, accelerate
        if (deltaTime < halfwayTime)
        {
            // Note that we need to calculate result before updating velocity, as it depends on velocity, but we don't want to use the updated value
            var result = Displacement(current, velocity, a1, deltaTime);
            velocity = Velocity(velocity, deltaTime, a1);
            return result;
        }
        else
        {
            // Greater than halfway, decelerate
            // Overshoot check, if we're going to reach the target before deltaTime, simply return the force to reach the target, effectively limiting acceleration.
            // This is required due to discrete physics, we have to apply acceleration for a whole timestep, we can't stop halfway-through.
            var arrivalTime = (2 * sqrtDiscriminant - vs) / acceleration;
            if (arrivalTime <= deltaTime)
            {
                velocity = 0;
                return target;
            }

            // New velocity is the halfway velocity, followed by decelerating based on how much time has passed since the halfway velocity
            var halfwayVelocity = Velocity(velocity, halfwayTime, a1);

            // Similarly for final position, we compute the distance travelled from the halfway position at our current time, offset by the halfway time
            var halfwayPosition = Displacement(current, velocity, a1, halfwayTime);

            // Note that we need to calculate result before updating velocity, as it depends on velocity, but we don't want to use the updated value
            var result = Displacement(halfwayPosition, halfwayVelocity, -a1, deltaTime - halfwayTime);
            velocity = Velocity(halfwayVelocity, deltaTime - halfwayTime, -a1);
            return result;
        }
    }

    public static float MoveTowards(float current, float target, ref float velocity, float acceleration)
    {
        return MoveTowards(current, target, ref velocity, acceleration, Time.deltaTime);
    }

    public static float MoveTowardsForce(float current, float target, float velocity, float acceleration, float deltaTime)
    {
        // Note this is a simplified version. The correct version will need to account for discrete timestep formulas
        var c = target < current ? -Sqrt(2 * acceleration * (current - target)) : Sqrt(2 * acceleration * (target - current));
        var a1 = velocity < c ? acceleration : -acceleration;
        var vs = velocity < c ? velocity : -velocity;

        var sqrtDiscriminant = Sqrt(Max(0, 0.5f * Square(velocity) + a1 * (target - current)));
        var halfwayTime = (sqrtDiscriminant - vs) / acceleration;

        if (deltaTime < halfwayTime)
        {
            // Solving v = v0+at for a gives us a=(v-v0)/t
            var newVelocity = Velocity(velocity, deltaTime, a1);
            return (newVelocity - velocity) / deltaTime;
        }
        else
        {
            var arrivalTime = (2 * sqrtDiscriminant - vs) / acceleration;
            if (arrivalTime <= deltaTime)
            {
                // To reach our final position we need to cover the remaining distance in one timestep
                return (target - current) / deltaTime;
            }

            // New velocity is the halfway velocity, followed by decelerating based on how much time has passed since the halfway velocity
            var halfwayVelocity = Velocity(velocity, halfwayTime, a1);
            var newVelocity = Velocity(halfwayVelocity, deltaTime - halfwayTime, -a1);

            // Solving v = v0+at for a gives us a=(v-v0)/t
            return (newVelocity - velocity) / deltaTime;
        }
    }
}
