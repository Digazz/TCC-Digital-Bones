using System;
using UnityEngine;

/// <summary>Catalog metadata for one validated anatomical model.</summary>
[CreateAssetMenu(menuName = "Digital Bones/Anatomy Model", fileName = "AnatomyModel")]
public sealed class AnatomyModelDefinition : ScriptableObject
{
    [SerializeField] private string modelId;
    [SerializeField] private string displayName;
    [SerializeField] private string species;
    [SerializeField] private string bone;
    [SerializeField, TextArea] private string description;
    [SerializeField] private GameObject modelPrefab;
    [SerializeField] private Vector3 physicalDimensionsMillimeters;
    [SerializeField] private string sourceStudyId;
    [SerializeField] private ModelValidationStatus validationStatus;

    public string ModelId => modelId;
    public string DisplayName => displayName;
    public string Species => species;
    public string Bone => bone;
    public string Description => description;
    public GameObject ModelPrefab => modelPrefab;
    public Vector3 PhysicalDimensionsMillimeters => physicalDimensionsMillimeters;
    public string SourceStudyId => sourceStudyId;
    public ModelValidationStatus ValidationStatus => validationStatus;
}

public enum ModelValidationStatus
{
    Placeholder,
    InReview,
    Validated
}

[Serializable]
public sealed class AnatomicalLabel
{
    [SerializeField] private string labelId;
    [SerializeField] private string displayName;
    [SerializeField, TextArea] private string explanation;
    [SerializeField] private Transform anchor;

    public string LabelId => labelId;
    public string DisplayName => displayName;
    public string Explanation => explanation;
    public Transform Anchor => anchor;
}
