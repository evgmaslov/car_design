import re
from tqdm.auto import tqdm

def code_to_json(code):
    code = re.sub("//(?:.|\s)+?(?=\n)", "", code)
    float_parameters = ["Length", "Width", "Height", "WheelWidth", "WheelRadius", "WheelRelativeBiasAlongWidth", "GapBetweenWheelAndBase"]
    serialized = {}
    for param in float_parameters:
        float_pattern = "(?<=" + param + ")\s*?=\s*?[\d\.]+?(?=f)"
        value = re.findall(float_pattern, code)[0]
        serialized[param] = float(value.replace("=", "").strip())

    param = "WheelRelativeBiasesAlongLength"
    pattern = "(?<=" + param + ")(?:.|\n)+?(?=;)"
    value_list = re.findall(pattern, code)[0]
    value_list = re.findall("(?<=\{)(?:.|\n)+?(?=\})", value_list)[0].split(",")
    values = []
    for value in value_list:
        value = re.findall(".+?(?=f)", value)[0].strip()
        values.append(float(value))
    serialized[param] = values

    params = ["WheelBaseSegmentsSpans", "BodySegmentsSpans"]
    for param in params:
        pattern = "(?<=" + param + ")\s*?=\s*?new\s+?List\s*?<\s*?List\s*?<\s*?float\s*?>\s*?>\s*?\(\s*?\)\s*?\{((?:.|\n)+?)\}\s*?;"
        list_of_lists = re.findall(pattern, code)[0]
        pattern = "(?<=\{)(?:.|\n)+?(?=\})"
        float_lists = re.findall(pattern, list_of_lists)
        total_lists = []
        for l in float_lists:
            floats = l.split(",")
            float_values = []
            for f in floats:
                value = re.findall(".+?(?=f)", f)[0].strip()
                float_values.append(float(value))
            total_lists.append(float_values)
        serialized[param] = total_lists

    params = ["WheelBaseSegmentsBottomSurfaces", "BodySegmentsTopSurfaces"]
    for param in params:
        pattern = "(?<=" + param + ")(?:.|\n)+?(?=;)"
        classes = re.findall(pattern, code)[0]
        classes = re.findall("(?<=\{)(?:.|\n)+?(?=\})", classes)[0]
        classes = re.findall("new\s+?\w+?\s*?\((?:.|\n)+?\)", classes)
        class_jsons = []
        for c in classes:
            class_json = []
            pattern = "(?<=new).+?(?=\()"
            class_name = re.findall(pattern, c)[0].strip()
            class_json.append(class_name)
            pattern = "(?<=\()(?:.|\n)+?(?=\))"
            parameters = re.findall(pattern, c)[0].split(",")
            parameters_json = {}
            for par in parameters:
                par = par.split(":")
                param_name = par[0].strip()
                param_value = re.findall("(?:\d|\.)+?(?=f)|\w+", par[1])[0].strip()
                try:
                    parameters_json[param_name] = float(param_value)
                except:
                    parameters_json[param_name] = param_value
            class_json.append(parameters_json)
            class_jsons.append(class_json)
        serialized[param] = class_jsons

    param = "WheelBaseTopSurface"
    class_json = []
    pattern = "(?<=" + param + ").+?(?=;)"
    class_value = re.findall(pattern, code)[0]
    pattern = "(?<=new).+?(?=\()"
    class_name = re.findall(pattern, class_value)[0].strip()
    class_json.append(class_name)
    pattern = "(?<=\().+?(?=\))"
    parameters = re.findall(pattern, class_value)[0].split(",")
    parameters_json = {}
    for par in parameters:
        par = par.split(":")
        param_name = par[0].strip()
        param_value = re.findall(".+?(?=f)|\s*?\w+?\s*?", par[1])[0].strip()
        try:
            parameters_json[param_name] = float(param_value)
        except:
            parameters_json[param_name] = param_value
    class_json.append(parameters_json)
    serialized[param] = class_json
    return serialized

def extract_codes(generations):
    codes = []
    pattern = "(?<=```csharp)(?:.|\s)+?(?=```)"
    for gen in tqdm(generations):
        code = re.findall(pattern, gen)
        if len(code) != 0:
            code = code[0]
            codes.append(code)
    return codes

def transform_codes_to_jsons(codes):
    inds = []
    jsons = []
    for ind, code in tqdm(enumerate(codes), total=len(codes)):
        try:
            code_json = code_to_json(code)
            jsons.append(code_json)
            inds.append(ind)
        except:
            continue
    return inds, jsons

def get_nearest_value(value, values_to_search):
    distance = 1e9
    nearest_value = 0
    for v in values_to_search:
        local_distance = abs(v - value)
        if local_distance < distance:
            distance = local_distance
            nearest_value = v
    return nearest_value

def get_parameter_value_part(code_json, indexes):
    value = code_json
    for ind in indexes:
        if type(value) == list:
            value = value[int(ind)]
        else:
            value = value[ind]
    return value

def get_indexes(value):
    indexes = []
    if type(value) == list:
        for i in range(len(value)):
            new_index = [i]
            local_value = value[i]
            local_indexes = get_indexes(local_value)
            if len(local_indexes) > 0:
                for local_index in local_indexes:
                    extended_index = new_index + local_index
                    indexes.append(extended_index)
            else:
                indexes.append(new_index)
    elif type(value) == dict:
        for k in value.keys():
            new_index = [k]
            local_value = value[k]
            local_indexes = get_indexes(local_value)
            if len(local_indexes) > 0:
                for local_index in local_indexes:
                    extended_index = new_index + local_index
                    indexes.append(extended_index)
            else:
                indexes.append(new_index)
    else:
        indexes.append([])
    return indexes

def drop_index_invalid(samples, indexes):
    valid_filtered_cars = []
    for car in samples:
        car_value = car
        is_valid = True
        for ind in indexes:
            try:
                value_part = get_parameter_value_part(car_value, ind)
            except:
                is_valid = False
                break
        if is_valid:
            valid_filtered_cars.append(car)
    return valid_filtered_cars

def index_to_key(index):
    key = "|".join([str(ind) for ind in index])
    return key

def key_to_index(key):
    ind = key.split("|")
    ind = [k for k in ind if len(str(k)) > 0]
    return ind

def is_equal_jsons(json1, json2):
    is_equal = True
    indexes1 = get_indexes(json1)
    indexes2 = get_indexes(json2)
    
    if len(indexes1) != len(indexes2):
        is_equal - False
    else:
        indexes2_keys = [index_to_key(ind) for ind in indexes2]
        for i in range(len(indexes1)):
            if index_to_key(indexes1[i]) not in indexes2_keys:
                is_equal = False
                break

    if is_equal:
        for ind in indexes1:
            value1 = get_parameter_value_part(json1, ind)
            value2 = get_parameter_value_part(json2, ind)
            if value1 != value2:
                is_equal = False
                break
    return is_equal

def get_valid_values(param, code_json, type_cars):
    parameters = ["Length", "Width", "Height", "WheelWidth", "WheelRadius", "WheelRelativeBiasAlongWidth", "WheelRelativeBiasesAlongLength", "WheelBaseSegmentsSpans", "WheelBaseSegmentsBottomSurfaces", "GapBetweenWheelAndBase", "WheelBaseTopSurface", "BodySegmentsSpans", "BodySegmentsTopSurfaces"]
    filtered_cars = type_cars
    global_indexes = get_indexes(code_json)
    for i, par in enumerate(parameters):
        if i < parameters.index(param):
            sample_value = code_json[par]
            rel_par_indexes = [[ind for ind in index] for index in global_indexes if index[0] == par]
            [index.pop(0) for index in rel_par_indexes]
            full_par_indexes = [[par] + index for index in rel_par_indexes]
            float_rel_par_indexes = [ind for ind in rel_par_indexes if type(get_parameter_value_part(sample_value, ind)) == float]
            str_rel_par_indexes = [ind for ind in rel_par_indexes if type(get_parameter_value_part(sample_value, ind)) == str]
            indexes = get_indexes(sample_value)
            valid_filtered_cars = drop_index_invalid(filtered_cars, full_par_indexes)
            new_filtered_cars = []
            nearest_values = {}
            for ind in float_rel_par_indexes:
                key = index_to_key(ind)
                value = get_parameter_value_part(sample_value, ind)
                values_to_search = [get_parameter_value_part(car[par], ind) for car in valid_filtered_cars]
                nearest_value = get_nearest_value(value, values_to_search)
                nearest_values[key] = nearest_value
            for car in valid_filtered_cars:
                is_car_valid = True
                car_value = car[par]
                for ind in float_rel_par_indexes:
                    sample_value_part = get_parameter_value_part(sample_value, ind)
                    car_value_part = get_parameter_value_part(car_value, ind)
                    if sample_value_part != car_value_part:
                        nearest_value = nearest_values[index_to_key(ind)]
                        if car_value_part != nearest_value:
                            is_car_valid = False
                            break
                for ind in str_rel_par_indexes:
                    sample_value_part = get_parameter_value_part(sample_value, ind)
                    car_value_part = get_parameter_value_part(car_value, ind)
                    if sample_value_part != car_value_part:
                        is_car_valid = False
                        break
                if is_car_valid:
                    new_filtered_cars.append(car)
            filtered_cars = new_filtered_cars 
        else:
            break
    valid_values = [car[param] for car in filtered_cars]
    unique_valid_values = []
    for v in valid_values:
        exists = False
        for u in unique_valid_values:
            if is_equal_jsons(v, u):
                exists = True
                break
        if not exists:
            unique_valid_values.append(v)
    return unique_valid_values

def is_from_distribution(value, valid_values):
    indexes = get_indexes(value)
    ind_valid_values = drop_index_invalid(valid_values, indexes)
    distributions = {}
    for ind in indexes:
        key = index_to_key(ind)
        if key not in distributions:
            distributions[key] = []
        for v in ind_valid_values:
            valid_value_part = get_parameter_value_part(v, ind)
            if value not in distributions[key]:
                distributions[key].append(valid_value_part)
    is_value_valid = True
    for key in distributions.keys():
        ind = key_to_index(key)
        dist = distributions[key]
        value_part = get_parameter_value_part(value, ind)
        if len(dist) == 0:
            is_value_valid = False
            break
        if type(value_part) == float:
            valid_min = min(dist)
            valid_max = max(dist)
            if value_part < valid_min or value_part > valid_max:
                is_value_valid = False
                break
        elif type(value_part) == str:
            if value_part not in dist:
                is_value_valid = False
                break
        else:
            is_value_valid = False
            break
    return is_value_valid

def define_car_type(code_json, codes_by_types):
    valid_types = []
    parameters = ["Length", "Width", "Height", "WheelWidth", "WheelRadius", "WheelRelativeBiasAlongWidth", "WheelRelativeBiasesAlongLength", "WheelBaseSegmentsSpans", "WheelBaseSegmentsBottomSurfaces", "GapBetweenWheelAndBase", "WheelBaseTopSurface", "BodySegmentsSpans", "BodySegmentsTopSurfaces"]
    types = ["sedan", "hatchback", "station wagon", "coupe", "limousine", "SUV", "pickup", "minivan", "van", "truck"]
    for car_type in types:
        type_cars = codes_by_types[car_type]
        passed = True
        for param in parameters:
            valid_values = get_valid_values(param, code_json, type_cars)
            if len(valid_values) == 0:
                passed = False
                break
            from_distribution = is_from_distribution(code_json[param], valid_values)
            if not from_distribution:
                passed = False
                break
        if passed:
            valid_types.append(car_type)
    return valid_types

def get_parameter(class_json, param):
    class_name = class_json[0]
    param_mapping = {
        "MaxHeight":"maxHeight",
        "MinHeight":"minHeight",
        "LeftRounded":"leftRounded",
        "CornerLength":"cornerRelativeLength",
        "TotalLength":"surfaceAbsoluteLength",
        "LeftCorner":"leftRoundedCorner",
        "RightCorner":"rightRoundedCorner"
    }
    if class_name == "Constant":
        param_mapping["MaxHeight"] = "height"
        param_mapping["MinHeight"] = "height"
    return class_json[1][param_mapping[param]]

def sample_with_drop(samples):
    results = [samples]
    if len(samples) > 1:
        for i in range(len(samples)):
            toDrop = [s for s in samples]
            toDrop.pop(i)
            droppedResults = sample_with_drop(toDrop)
            results.extend(droppedResults)
    return results

def num_to_str(num):
    string = ""
    if int(num) - num == 0:
        string = str(int(num))
    else:
        string = str(num)
    return string

def get_prompts_for_code(car, codes_by_types):
    car_types = define_car_type(car, codes_by_types)
    if len(car_types) == 0:
        return []
    allCars = []
    for key in codes_by_types.keys():
        allCars.extend(codes_by_types[key])

    for carType in car_types:
        numParameters = ["Length", "Width", "Height", "WheelRadius"]
        numParameterNames = ["length", "width", "height", "wheel radius"]
        classNumParameterBoundaries = {}
        totalNumParameterBoundaries = {}
        for param in numParameters:
            classNumParameterBoundaries[param] = [1e9, -1e9]
            totalNumParameterBoundaries[param] = [1e9, -1e9]
        for param in numParameters:
            for typeCar in codes_by_types[carType]:
                value = float(typeCar[param])
                if value < classNumParameterBoundaries[param][0]:
                    classNumParameterBoundaries[param][0] = value
                if value > classNumParameterBoundaries[param][1]:
                    classNumParameterBoundaries[param][1] = value
            for typeCar in allCars:
                value = float(typeCar[param])
                if value < totalNumParameterBoundaries[param][0]:
                    totalNumParameterBoundaries[param][0] = value
                if value > totalNumParameterBoundaries[param][1]:
                    totalNumParameterBoundaries[param][1] = value
        basePrompt = "Generate a car with the following characteristics: "
        
        prompts = []
    
        localTexts = []
        prompt = basePrompt
        textVariants = []
        carTypeTextVariants = []
        carTypeTextVariants.append(prompt)
        carTypeTextVariants.append(prompt + f"body type: {carType};")
        typeParameterDescriptions = []
        description = f"{len(car['WheelRelativeBiasesAlongLength'])} wheelsets"
        typeParameterDescriptions.append(description)
        if carType == "sedan" or carType == "coupe":
            typeParameterDescriptions.append("normal size bonnet")
            if car["BodySegmentsSpans"][2][0] == 0.8:
                typeParameterDescriptions.append("normal size boot")
            elif car["BodySegmentsSpans"][2][0] == 0.9:
                typeParameterDescriptions.append("small boot")
        elif carType == "hatchback" or carType == "SUV":
            typeParameterDescriptions.append("normal size bonnet")
            typeParameterDescriptions.append("normal size boot")
            typeParameterDescriptions.append("door at the rear")
        elif carType == "station wagon":
            typeParameterDescriptions.append("normal size bonnet")
            typeParameterDescriptions.append("large body")
            typeParameterDescriptions.append("large boot")
            typeParameterDescriptions.append("door at the rear")
        elif carType == "limousine":
            typeParameterDescriptions.append("normal size bonnet")
            if car["BodySegmentsSpans"][2][0] == 0.8:
                typeParameterDescriptions.append("normal size boot")
            elif car["BodySegmentsSpans"][2][0] == 0.9:
                typeParameterDescriptions.append("small boot")
            typeParameterDescriptions.append("very long body")
        elif carType == "pickup":
            typeParameterDescriptions.append("normal size bonnet")
            typeParameterDescriptions.append("large boot")
        elif carType == "minivan":
            typeParameterDescriptions.append("small bonnet")
            typeParameterDescriptions.append("large body")
            typeParameterDescriptions.append("no boot")
        elif carType == "van":
            typeParameterDescriptions.append("normal size bonnet")
            if float(get_parameter(car["BodySegmentsTopSurfaces"][2][1], "MaxHeight")) > float(get_parameter(car["BodySegmentsTopSurfaces"][1][1], "MaxHeight")):
                typeParameterDescriptions.append("1 cargo container")
            else:
                typeParameterDescriptions.append("1 flatbed")
        elif carType == "truck":
            if float(get_parameter(car["BodySegmentsTopSurfaces"][0][1], "MaxHeight")) > float(car["Height"]) * 0.6:
                typeParameterDescriptions.append("no bonnet")
            elif float(get_parameter(car["BodySegmentsTopSurfaces"][0][1], "MaxHeight")) > float(car["Height"]) * 0.6:
                typeParameterDescriptions.append("large bonnet")
            fairing = False
            for surf in car["BodySegmentsTopSurfaces"]:
                if surf[0] == "TotalRounded" and get_parameter(surf[1], "MaxHeight") == car["Height"]:
                    fairing = True
                    break
            if fairing:
                typeParameterDescriptions.append("roof fairing with sleeper cab")
            cargoCount = 0
            for i in range(len(car["BodySegmentsSpans"])):
                span = car["BodySegmentsSpans"][i]
                if span[1] - span[0] > 0.17:
                    cargoCount = len(car["BodySegmentsTopSurfaces"]) - i
                    break
            if cargoCount == 1:
                typeParameterDescriptions.append("1 cargo container")
            elif cargoCount > 1:
                typeParameterDescriptions.append(f"{cargoCount} cargo containers")
        descriptionSamples = sample_with_drop(typeParameterDescriptions)
        for sample in descriptionSamples:
            carTypeTextVariants.append(prompt + ";".join(sample))
        for carTypePrompt in carTypeTextVariants:
            textVariants.append(carTypePrompt)
            masks = []
            for j in range(2 ** len(numParameters)):
                mask = [None] * len(numParameters)
                div = j
                ind = len(numParameters) - 1
                while div > 0:
                    mask[ind] = div % 2
                    ind -= 1
                    div = int(div/2)
                for k in range(ind + 1):
                    mask[k] = 0
                masks.append(mask)
            for mask in masks:
                numParameterDescriptions = []
                for k in range(len(numParameters)):
                    info = numParameters[k]
                    value = float(car[info])
                    if mask[k] == 0:
                        numParameterDescriptions.append(f"{numParameterNames[k]}: {num_to_str(value)} cm")
                    else:
                        desc = ""
                        minv = 0
                        maxv = 0
                        if "body type:" in carTypePrompt:
                            minv = classNumParameterBoundaries[info][0]
                            maxv = classNumParameterBoundaries[info][1]
                        else:
                            minv = totalNumParameterBoundaries[info][0]
                            maxv = totalNumParameterBoundaries[info][1]
                        leftMedium = minv + (maxv - minv) / 3
                        rightMedium = minv + (maxv - minv) * 2 / 3
                        if value <= leftMedium:
                            desc = "small"
                        elif value >= rightMedium:
                            desc = "big"
                        else:
                            desc = "medium"
                        numParameterDescriptions.append(f"{numParameterNames[k]} is {desc}")
                numParameterDescriptionSamples = sample_with_drop(numParameterDescriptions)
                for sample in numParameterDescriptionSamples:
                    textVariants.append(f"{carTypePrompt}; {'; '.join(sample)}")
        corrected_prompts = []
        pattern = "(?<=;|:)\s*?;"
        for text in textVariants:
            cor = re.sub(pattern, "", text)
            corrected_prompts.append(cor)
        finalPrompts = []
        for text in corrected_prompts:
            if text not in finalPrompts:
                finalPrompts.append(text)
        prompts.extend(finalPrompts)
    return prompts

#measures the part of generations wich code can be extracted from
def extraction_score(generations):
    score = 0
    pattern = "(?<=```csharp)(?:.|\s)+?(?=```)"
    for gen in tqdm(generations):
        code = re.findall(pattern, gen)
        if len(code) != 0:
            code = code[0]
            score += 1
    score = score / len(generations)
    return score

#measures the part of generations wich code can be transformed to json. Generations should pass the extraction score before.
def compilation_score(codes):
    score = 0
    for code in tqdm(codes):
        try:
            code_json = code_to_json(code)
            score += 1
        except:
            continue
    score = score / len(codes)
    return score

#measures the part of generations wich compilled jsons can be transformed to templated text inputs correctly
def reconstruction_score(jsons, descriptions, codes_by_types):
    score = 0
    total_samples = 0
    for ind, code_json in tqdm(enumerate(jsons), total = len(jsons), desc="Computing score"):
        desc = descriptions[ind]
        pattern = "(?<=;|:)\s*?;"
        desc = re.sub(pattern, "", desc)
        total_samples += 1
        prompts = get_prompts_for_code(code_json, codes_by_types)
        if desc in prompts:
            score += 1
    score = score / total_samples
    return score

