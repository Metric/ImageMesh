#pragma once

#include "CoreMinimal.h"
#include "CoreMisc.h"
#include "Engine.h"
#include "FileManager.h"
#include "Components/ActorComponent.h"
#include "IMeshReader.generated.h"

UCLASS( ClassGroup=(Custom), meta=(BlueprintSpawnableComponent) )
class UIMeshReader : public UActorComponent
{
	GENERATED_BODY()

public:	
	// Sets default values for this component's properties
	UIMeshReader();

protected:
	// Called when the game starts
	virtual void BeginPlay() override;

	void ParseVertex(FString line);
	void ParseTriangle(FString line);

public:	
	// Called every frame
	virtual void TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) override;

	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "imesh")
	TArray<FVector> Vertices;

	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "imesh")
	TArray<FVector> Normals;

	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "imesh")
	TArray<int32> Indices;

	UPROPERTY(EditAnywhere, BlueprintReadOnly, Category = "imesh")
	TArray<FLinearColor> Colors;

	UFUNCTION(BlueprintCallable, Category = "imesh")
	void ParseFile(FString name);
};
