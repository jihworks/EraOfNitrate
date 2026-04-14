#ifndef INDIRECT_INSTANCING_INCLUDED
#define INDIRECT_INSTANCING_INCLUDED

// Transform matrix buffer.
StructuredBuffer<float4x4> _InstanceTransforms;

void ApplyIndirectTransform_float(float3 objectPosition, float3 objectNormal, float instanceID, out float3 outPosition, out float3 outNormal)
{
#if SHADERGRAPH_PREVIEW
    // Default values for preview.
    outPosition = objectPosition;
    outNormal = objectNormal;
#else
    uint id = (uint)instanceID;
    // Get transform by Instance ID
    float4x4 transformMatrix = _InstanceTransforms[id];

    float3 actualWorldPosition = mul(transformMatrix, float4(objectPosition, 1.0)).xyz;
    float3 actualWorldNormal = mul((float3x3)transformMatrix, objectNormal);

    // Invalidate shader graph transforms.
    outPosition = mul(UNITY_MATRIX_I_M, float4(actualWorldPosition, 1.0)).xyz;
    outNormal = mul((float3x3)UNITY_MATRIX_I_M, actualWorldNormal);
#endif
}

void ApplyIndirectTransform_half(float3 objectPosition, float3 objectNormal, float instanceID, out float3 outPosition, out float3 outNormal)
{
#if SHADERGRAPH_PREVIEW
    outPosition = objectPosition;
    outNormal = objectNormal;
#else
    uint id = (uint)instanceID;
    float4x4 transformMatrix = _InstanceTransforms[id];

    float3 actualWorldPosition = mul(transformMatrix, float4(objectPosition, 1.0)).xyz;
    float3 actualWorldNormal = mul((float3x3)transformMatrix, objectNormal);

    outPosition = mul(UNITY_MATRIX_I_M, float4(actualWorldPosition, 1.0)).xyz;
    outNormal = mul((float3x3)UNITY_MATRIX_I_M, actualWorldNormal);
#endif
}

#endif