#include "IMeshReader.h"

// Sets default values for this component's properties
UIMeshReader::UIMeshReader()
{
	// Set this component to be initialized when the game starts, and to be ticked every frame.  You can turn these features
	// off to improve performance if you don't need them.
	PrimaryComponentTick.bCanEverTick = false;

	// ...
}

// Called when the game starts
void UIMeshReader::BeginPlay()
{
	Super::BeginPlay();

	// ...
}

// Called every frame
void UIMeshReader::TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction)
{
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);

	// ...
}

void UIMeshReader::ParseFile(FString name)
{
	Normals = TArray<FVector>();
	Vertices = TArray<FVector>();
	Indices = TArray<int32>();
	Colors = TArray<FLinearColor>();

	FString fpath = FPaths::Combine(FPaths::GameContentDir(), name);
	FString result = TEXT("");
	IPlatformFile& file = FPlatformFileManager::Get().GetPlatformFile();
	
	if (file.FileExists(*fpath))
	{
		FFileHelper::LoadFileToString(result, *fpath);
		TArray<FString> lines;
		result.ParseIntoArrayLines(lines);

		bool inVerts = false;
		bool inTris = false;
		for (FString line : lines) 
		{
			if (line.ToLower().Contains("vertices"))
			{
				inVerts = true;
				inTris = false;
			}
			else if (line.ToLower().Contains("triangles"))
			{
				inVerts = false;
				inTris = true;
			}

			if (inVerts)
			{
				ParseVertex(line);
			}
			else if (inTris)
			{
				ParseTriangle(line);
			}
		}
	}
}

void UIMeshReader::ParseVertex(FString line)
{
	TArray<FString> vertData;
	FString delim = TEXT(" ");
	line.ParseIntoArray(vertData, *delim);

	if (vertData.Num() != 9) return;

	float x = 0;
	float y = 0;
	float z = 0;

	float nx = 0;
	float ny = 0;
	float nz = 0;

	float r = 0;
	float g = 0;
	float b = 0;

	//read backwards as we pop the array
	b = FCString::Atof(*vertData.Pop());
	g = FCString::Atof(*vertData.Pop());
	r = FCString::Atof(*vertData.Pop());

	nz = FCString::Atof(*vertData.Pop());
	ny = FCString::Atof(*vertData.Pop());
	nx = FCString::Atof(*vertData.Pop());

	z = FCString::Atof(*vertData.Pop());
	y = FCString::Atof(*vertData.Pop());
	x = FCString::Atof(*vertData.Pop());

	//convert rgb to linearcolor space
	FLinearColor c = FLinearColor(r, g, b);
	Colors.Add(c);

	Vertices.Add(FVector(x,y,z));

	Normals.Add(FVector(nx, ny, nz));
}

void UIMeshReader::ParseTriangle(FString line)
{
	TArray<FString> triData;
	FString delim = TEXT(" ");
	line.ParseIntoArray(triData, *delim);

	if (triData.Num() != 3) return;

	int32 i3 = FCString::Atoi(*triData.Pop());
	int32 i2 = FCString::Atoi(*triData.Pop());
	int32 i1 = FCString::Atoi(*triData.Pop());

	Indices.Add(i1);
	Indices.Add(i2);
	Indices.Add(i3);
}
