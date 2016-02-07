using UnityEngine;
using System.Collections;
using System;

public class ValueNoise {

    // Random value for x position using seed
    static float Value ( int x, int seed ) {
        // Bit shift x 13 places left and set it to the power of itself
        x = ( x << 13 ) ^ x;
        // Here are the numbers used and what they do in order
        // 15731 = a base scaler amount used to add to x
        // 2147483647 = the max integer value possible, used to invert seed
        // 789221 = a base offset for the value plus our seed
        // 1376312589 = another base offset used
        // 2147483647 = used as a modulus operator to clamp the value in the integers max value range
        // 1073741824 = used as a divisor to get in the general range of -1 and 1
        // Lastly we clamp to make sure it is between -1 and 1
        return Mathf.Clamp ( ( 1.0f - ( ( x * ( x * x * ( 15731 + ( 2147483647 - seed ) ) + ( 789221 + seed ) ) + 1376312589 ) & 2147483647 ) / 1073741824.0f ), -1.0f, 1.0f );   
    }

    // Random value for x and y positions using seed and our first value function
    static float Value ( int x, int y, int seed ) {
        return Value ( x + y * 57, seed );
    }

    // Simple cosine interpolation
    static float Interpolate ( float a, float b, float t ) {
        // We get our angle relative to the top of the circle
        double angle = t * Math.PI;
        // We then get our length using cosine
        t = ( float ) ( ( 1d - Math.Cos ( angle ) ) * 0.5d );
        // Lasting we use the length to figure out where inbetween a and b we are
        return a * ( 1 - t ) + b * t;
    }

    // Round noise value using its neighbours
    static float Rounded1D ( int x, int seed ) {
        // We get partial heights from our neighbours and dampen our own while adding all
        return Value ( x, seed ) / 2 + Value ( x - 1, seed ) / 4 + Value ( x + 1, seed ) / 4;
    }

    // Round noise value using its neighbours
    static float Rounded2D ( int x, int y, int seed ) {
        // The corners of our 2D square have the least affect on our value
        float corners = ( Value ( x - 1, y - 1, seed ) + Value ( x + 1, y - 1, seed ) + Value ( x - 1, y + 1, seed ) + Value ( x + 1, y + 1, seed ) ) / 16f;
        // The sides give a little more due to them being closer
        float sides = ( Value ( x - 1, y, seed ) + Value ( x + 1, y, seed ) + Value ( x, y - 1, seed ) + Value ( x, y + 1, seed ) ) / 8f;
        // Add our sides and corners to our diminished center
        return corners + sides + Value ( x, y, seed ) / 4f;
    }

    // Used to get exact x coordinate not just an integer. Samples from closest x integers
    static float Smoothed1D ( float x, int seed ) {
        // Gets the integer to the left of our x
        int intX = Mathf.FloorToInt ( x );
        // The distance between our x and our integer to the left
        float fracX = x - intX;
        // We get the noise value of our left integer
        float x0 = Rounded1D ( intX, seed );
        // We also get the noise of our right integer
        float x1 = Rounded1D ( intX + 1, seed );
        // And we get the noise inbetween those using the distance from our left int as the percent
        return Interpolate ( x0, x1, fracX );
    }

    // Used to get exact x and y coordinates not just integers. Samples from closest x and y integers
    static float Smoothed2D ( float x, float y, int seed ) {
        // Gets the integer to the left of our x
        int intX = Mathf.FloorToInt ( x );
        // The distance between our x and our integer to the left
        float fracX = x - intX;
        // Gets the integer below of our y
        int intY = Mathf.FloorToInt ( y );
        // The distance between our y and our integer below us
        float fracY = y - intY;
        // We get the noise value of our left integer
        float x0 = Rounded2D ( intX, intY, seed );
        // We also get the noise of our right integer
        float x1 = Rounded2D ( intX + 1, intY, seed );
        // We get the noise value of our top integer
        float y0 = Rounded2D ( intX, intY + 1, seed );
        // We also get the noise of our right and top integer
        float y1 = Rounded2D ( intX + 1, intY + 1, seed );
        // Our points value on the lower x axis
        float i0 = Interpolate ( x0, x1, fracX );
        // Our points value on the upper x axis
        float i1 = Interpolate ( y0, y1, fracX );
        // And we get the noise inbetween those by moving on our y
        return Interpolate ( i0, i1, fracY );
    }

    // What other scripts will call to get a value clampted at the extremes of amplitude
    public static float Value1D ( float x, int seed, int octaves, float frequency, float amplitude ) {
        // How much we shrink the height of our noise and how much we increase our frequency by
        float gain = 1.0f;
        // The total sum of the noise
        float sum = 0.0f;
        // Loop though each octave and add to sum and gain
        for ( int i = 0; i < octaves; i++ ) {
            // Add our noise at the correct frequency and the correct height
            sum += Smoothed1D ( x * gain / frequency, seed ) * amplitude / gain;
            // Continue to add to gain to increase octave frequency and lower octave height
            gain *= 2.0f;
        }
        // Lastly return sum
        return sum;
    }

    // What other scripts will call to get a value clampted at the extremes of amplitude
    public static float Value2D ( float x, float y, int seed, int octaves, float frequency, float amplitude ) {
        // How much we shrink the height of our noise and how much we increase our frequency by
        float gain = 1.0f;
        // The total sum of the noise
        float sum = 0.0f;
        // Loop though each octave and add to sum and gain
        for ( int i = 0; i < octaves; i++ ) {
            // Add our noise at the correct frequency and the correct height
            sum += Smoothed2D ( x * gain / frequency, y * gain / frequency, seed ) * amplitude / gain;
            // Continue to add to gain to increase octave frequency and lower octave height
            gain *= 2.0f;
        }
        // Lastly return sum
        return sum;
    }
}
